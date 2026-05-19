<?php
namespace Api;

use Classes\Validate;
use Core\JWToken;
use Core\Response;
use Core\Route;
use Exception;
use Models\Users;
use Models\UsersQuery;
use OpenApi\Attributes as OA;
use Propel\Runtime\Map\TableMap;

#[OA\Info(
    version: '1.0.0',
    title: 'Volunteer API',
    description: 'API для волонтёрского приложения. Управление пользователями, задачами, проектами и чатами.'
)]
#[OA\Server(
    url: 'http://localhost:8080',
    description: 'Локальный сервер'
)]
#[OA\SecurityScheme(
    securityScheme: 'cookieAuth',
    type: 'apiKey',
    in: 'cookie',
    name: 'jwt',
    description: 'Аутентификация через JWT в cookie (HttpOnly)'
)]
#[OA\Tag(name: 'Users', description: 'Управление пользователями')]
#[Route("/api/users")]
class UserController
{

    #[OA\Get(
        path: '/api/users/search',
        tags: ['Users'],
        summary: 'Поиск пользователей по имени, телефону или email',
        parameters: [
            new OA\Parameter(
                name: 'q',
                in: 'query',
                required: true,
                schema: new OA\Schema(type: 'string'),
                description: 'Строка поиска (по full_name, phone, email)'
            ),
            new OA\Parameter(
                name: 'limit',
                in: 'query',
                required: false,
                schema: new OA\Schema(type: 'integer', default: 10),
                description: 'Количество результатов'
            ),
            new OA\Parameter(
                name: 'offset',
                in: 'query',
                required: false,
                schema: new OA\Schema(type: 'integer', default: 0),
                description: 'Смещение для пагинации'
            )
        ],
        responses: [
            new OA\Response(
                response: 200,
                description: 'Список найденных пользователей',
                content: new OA\JsonContent(
                    type: 'array',
                    items: new OA\Items(ref: '#/components/schemas/UserProfile')
                )
            ),
            new OA\Response(
                response: 400,
                description: 'Не указан параметр q',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            )
        ]
    )]
    #[Route("/search", "GET", false)]
    public function searchUsers($params)
    {
        try {
            $q = $params['q'] ?? null;
            if (!$q || trim($q) === '') {
                return new Response(400, ['error' => 'Search query required']);
            }

            $limit = isset($params['limit']) ? (int)$params['limit'] : 10;
            $offset = isset($params['offset']) ? (int)$params['offset'] : 0;

            $likePattern = '%' . $q . '%';
            $users = UsersQuery::create()
                ->filterByFullName($likePattern, \Propel\Runtime\ActiveQuery\Criteria::LIKE)
                ->_or()
                ->filterByPhone($likePattern, \Propel\Runtime\ActiveQuery\Criteria::LIKE)
                ->_or()
                ->filterByEmail($likePattern, \Propel\Runtime\ActiveQuery\Criteria::LIKE)
                ->limit($limit)
                ->offset($offset)
                ->find();

            $result = [];
            foreach ($users as $user) {
                $result[] = $this->formatUserProfile($user);
            }

            return new Response(200, $result);
        } catch (\Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Регистрация нового пользователя
     */
    #[OA\Post(
        path: '/api/users/register',
        tags: ['Users'],
        summary: 'Регистрация нового пользователя',
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                required: ['full_name', 'password', 'confirm'],
                properties: [
                    new OA\Property(property: 'full_name', type: 'string', example: 'Иван Иванов'),
                    new OA\Property(property: 'phone', type: 'string', example: '+79001234567'),
                    new OA\Property(property: 'email', type: 'string', format: 'email', example: 'ivan@example.com'),
                    new OA\Property(property: 'password', type: 'string', format: 'password', example: 'secret'),
                    new OA\Property(property: 'confirm', type: 'string', format: 'password', example: 'secret'),
                    new OA\Property(property: 'city', type: 'string', example: 'Пермь'),
                    new OA\Property(property: 'birth_date', type: 'string', format: 'date', example: '2000-05-15'),
                    new OA\Property(property: 'about', type: 'string', example: 'О себе')
                ]
            )
        ),
        responses: [
            new OA\Response(
                response: 200,
                description: 'Успешная регистрация',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'result', type: 'string', example: 'successful')
                    ]
                )
            ),
            new OA\Response(
                response: 400,
                description: 'Ошибка валидации (не все параметры, пароли не совпадают, неверный формат)',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 409,
                description: 'Пользователь с таким телефоном или email уже существует',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            )
        ]
    )]
    #[Route("/register", "POST", false)]
    public function register($params)
    {
        try {
            // Проверяем обязательные поля
            $required = ['full_name', 'password', 'confirm'];
            if (Validate::checkParams($required, $params)) {
                return new Response(400, ['error' => 'Missing required parameters']);
            }

            // Проверка совпадения паролей
            if ($params['password'] !== $params['confirm']) {
                return new Response(400, ['error' => 'Passwords do not match']);
            }

            // Должен быть указан хотя бы телефон или email
            $phone = isset($params['phone']) ? trim($params['phone']) : null;
            $email = isset($params['email']) ? trim($params['email']) : null;
            if (!$phone && !$email) {
                return new Response(400, ['error' => 'Either phone or email must be provided']);
            }

            // Валидация форматов, если они переданы
            if ($phone && Validate::phone($phone, $r)) {
                return $r;
            }
            if ($email && Validate::email($email, $r)) {
                return $r;
            }

            // Проверка уникальности телефона и email
            if ($email && Validate::findUserByField('Email', $email) || $phone && Validate::findUserByField('Phone', $phone, $r)) {
                return new Response(409, ['error' => 'User with this email or phone already exists']);
            }

            // Валидация даты рождения, если передана
            if (isset($params['birth_date']) && !Validate::date($params['birth_date'], $r)) {
                return $r;
            }

            // Создание пользователя
            $user = new Users();
            $user->setFullName(trim($params['full_name']));
            $user->setPassword(password_hash($params['password'], PASSWORD_DEFAULT));

            if ($phone) {
                $user->setPhone($phone);
            }
            if ($email) {
                $user->setEmail($email);
            }
            if (isset($params['city'])) {
                $user->setCity(trim($params['city']));
            }
            if (isset($params['birth_date'])) {
                $user->setBirthDate($params['birth_date']); // Propel setter ожидает строку или DateTime
            }
            if (isset($params['about'])) {
                $user->setAbout(trim($params['about']));
            }
            // Поля по умолчанию
            $user->setBalance(0.00);
            $user->setAvailabilityStatus('available');
            $user->setIsAdmin(false);
            $user->setIsModerator(false);
            $user->setIsOnline(false);
            $user->setAverageRating(0.00);

            $user->save();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Вход в систему
     */
    #[OA\Post(
        path: '/api/users/signin',
        tags: ['Users'],
        summary: 'Авторизация пользователя',
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                required: ['login', 'password'],
                properties: [
                    new OA\Property(property: 'login', type: 'string', example: '+79001234567', description: 'Телефон или email'),
                    new OA\Property(property: 'password', type: 'string', format: 'password', example: 'secret')
                ]
            )
        ),
        responses: [
            new OA\Response(
                response: 200,
                description: 'Успешный вход. В cookie устанавливается JWT, в теле возвращаются данные пользователя',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'success', type: 'object',
                            properties: [
                                new OA\Property(property: 'id', type: 'integer'),
                                new OA\Property(property: 'phone', type: 'string', nullable: true),
                                new OA\Property(property: 'email', type: 'string', nullable: true),
                                new OA\Property(property: 'full_name', type: 'string')
                            ]
                        )
                    ]
                )
            ),
            new OA\Response(
                response: 400,
                description: 'Неверный логин/пароль или отсутствуют параметры',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            )
        ]
    )]
    #[Route("/signin", "POST", false)]
    public function signIn($params)
    {
        try {
            $login = $params['login'] ?? null;
            $password = $params['password'] ?? null;

            if (!$login || !$password) {
                return new Response(400, ['error' => 'Login and password required']);
            }

            // Определяем, что пришло: телефон или email
            $user = null;
            $r = new Response();
            if (!Validate::phone($login, $r)) {
                $user = UsersQuery::create()->findOneByPhone($login);
            } elseif (!Validate::email($login, $r)) {
                $user = UsersQuery::create()->findOneByEmail($login);
            } else {
                return new Response(400, ['error' => 'Invalid login format']);
            }

            if (!$user) {
                return new Response(400, ['error' => 'Wrong login or password']);
            }

            if (!password_verify($password, $user->getPassword())) {
                return new Response(400, ['error' => 'Wrong login or password']);
            }

            // Обновляем время последнего визита и статус онлайн
            $user->setLastSeenAt(new \DateTime());
            $user->setIsOnline(true);
            $user->save();

            $payload = [
                'id' => $user->getId(),
                'phone' => $user->getPhone(),
                'email' => $user->getEmail(),
                'full_name' => $user->getFullName()
            ];

            $token = JWToken::generateToken($payload);

            $response = new Response(200, $this->formatUserProfile($user));
            $response->setCook(
                'jwt',
                $token,
                time() + (60 * 60 * 6),
                '/',
                '',
                false,
                true,
                'Strict'
            );

            session_start();
            $_SESSION = $payload;

            return $response;
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Получение публичной информации о пользователе по ID
     */
    #[OA\Get(
        path: '/api/users/{id}',
        tags: ['Users'],
        summary: 'Получить публичные данные пользователя по ID',
        parameters: [
            new OA\Parameter(
                name: 'id',
                in: 'path',
                required: true,
                schema: new OA\Schema(type: 'integer'),
                description: 'ID пользователя'
            )
        ],
        responses: [
            new OA\Response(
                response: 200,
                description: 'Публичные данные пользователя',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'id', type: 'integer'),
                        new OA\Property(property: 'full_name', type: 'string'),
                        new OA\Property(property: 'city', type: 'string', nullable: true),
                        new OA\Property(property: 'birth_date', type: 'string', format: 'date', nullable: true),
                        new OA\Property(property: 'about', type: 'string', nullable: true),
                        new OA\Property(property: 'profile_picture', type: 'string', nullable: true),
                        new OA\Property(property: 'availability_status', type: 'string', enum: ['available', 'busy', 'offline']),
                        new OA\Property(property: 'average_rating', type: 'number', format: 'float'),
                        new OA\Property(property: 'last_seen_at', type: 'string', format: 'date-time', nullable: true),
                        new OA\Property(property: 'is_online', type: 'boolean')
                    ]
                )
            ),
            new OA\Response(
                response: 400,
                description: 'ID не передан',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 404,
                description: 'Пользователь не найден',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            )
        ]
    )]
    #[Route("/{id}", "GET")]
    public function getUserById($params)
    {
        try {
            $id = $params['id'] ?? null;

            if (!$id) {
                return new Response(400, ['error' => 'User ID required']);
            }

            if($id == -1){
                $id = $_SESSION['id'];
            }

            $user = UsersQuery::create()->findOneById($id);
            if (!$user) {
                return new Response(404, ['error' => 'User not found']);
            }

            return new Response(200, $this->formatUserProfile($user));
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Редактирование профиля
     */
    #[OA\Put(
        path: '/api/users/{Id}',
        tags: ['Users'],
        summary: 'Редактирование профиля текущего пользователя',
        parameters: [
            new OA\Parameter(
                name: 'id',
                in: 'path',
                required: true,
                schema: new OA\Schema(type: 'integer'),
                description: 'ID пользователя'
            )
        ],
        security: [['cookieAuth' => []]],
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                properties: [
                    new OA\Property(property: 'full_name', type: 'string', example: 'Пётр Петров'),
                    new OA\Property(property: 'phone', type: 'string', example: '+79007654321'),
                    new OA\Property(property: 'email', type: 'string', format: 'email', example: 'petr@example.com'),
                    new OA\Property(property: 'city', type: 'string', example: 'Москва'),
                    new OA\Property(property: 'birth_date', type: 'string', format: 'date', example: '1995-03-20'),
                    new OA\Property(property: 'about', type: 'string', example: 'Новый текст о себе'),
                    new OA\Property(property: 'profile_picture', type: 'string', example: 'https://example.com/avatar.jpg'),
                    new OA\Property(property: 'availability_status', type: 'string', enum: ['available', 'busy', 'offline'])
                ]
            )
        ),
        responses: [
            new OA\Response(
                response: 200,
                description: 'Профиль обновлён',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'result', type: 'string', example: 'successful')
                    ]
                )
            ),
            new OA\Response(
                response: 400,
                description: 'Ошибка валидации (неверный формат, занятый телефон/email и т.д.)',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 401,
                description: 'Не авторизован',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 404,
                description: 'Пользователь не найден',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 409,
                description: 'Телефон или email уже заняты',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            )
        ]
    )]
    #[Route("/{id}", "PUT")]
    public function edit($params)
    {
        try {
            $userId = $params['id'] ?? null;
           
            if (!$userId) {
                return new Response(400, ['error' => 'User ID required']);
            }

            if($userId == -1){
                $userId = $_SESSION['id'];
            }

            if (($_SESSION['id'] ?? null) != $userId) {
                return new Response(403, ['error' => 'You can only redact your own profile']);
            }

            $user = UsersQuery::create()->findOneById($userId);
            if (!$user) {
                return new Response(404, ['error' => 'User not found']);
            }

            // Проверка текущего пароля
            $currentPassword = $params['current_password'] ?? null;
            if (!$currentPassword || !password_verify($currentPassword, $user->getPassword())) {
                return new Response(403, ['error' => 'Invalid password']);
            }

            // Обновление полей
            $editableFields = [
                'full_name',
                'city',
                'birth_date',
                'email',
                'phone',
                'about',
                'profile_picture',
                'education_institution',
                'education_degree',
                'education_field',
                'education_start_year',
                'education_end_year'];

            if (isset($params['phone']) && trim($params['phone']) !== $user->getPhone()) {
                $phone = trim($params['phone']);
                if (Validate::phone($phone, $r)) return $r;
                if (Validate::findUserByField('Phone', $phone, $r, $userId)) return new Response(409, ['error' => 'User with this phone already exists']);
            }

            if (isset($params['email']) && trim($params['email']) !== $user->getEmail()) {
                $email = trim($params['email']);
                if (Validate::email($email, $r)) return $r;
                if (Validate::findUserByField('Email', $email, $r, $userId)) return new Response(409, ['error' => 'User with this email already exists']);
                $user->setEmail($email);
            }

            foreach ($editableFields as $field) {
                if (isset($params[$field])) {
                    $v = trim($params[$field]);
                    if($v != $user->getByName($field, TableMap::TYPE_FIELDNAME)){
                        $user->setByName($field, $v, TableMap::TYPE_FIELDNAME);
                    }
                }
            }

            $user->save();
            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Смена пароля
     */
    #[OA\Put(
        path: '/api/users/password',
        tags: ['Users'],
        summary: 'Смена пароля текущего пользователя',
        security: [['cookieAuth' => []]],
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                required: ['old', 'new', 'confirm'],
                properties: [
                    new OA\Property(property: 'old', type: 'string', format: 'password', example: 'oldpass'),
                    new OA\Property(property: 'new', type: 'string', format: 'password', example: 'newpass'),
                    new OA\Property(property: 'confirm', type: 'string', format: 'password', example: 'newpass')
                ]
            )
        ),
        responses: [
            new OA\Response(
                response: 200,
                description: 'Пароль изменён',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'result', type: 'string', example: 'successful')
                    ]
                )
            ),
            new OA\Response(
                response: 400,
                description: 'Не все поля, не совпадают, неверный старый пароль',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 401,
                description: 'Не авторизован',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 404,
                description: 'Пользователь не найден',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            )
        ]
    )]
    #[Route("/password", "PUT")]
    public function changePassword($params)
    {
        try {
            if(!session_status() == PHP_SESSION_NONE){
                session_start();
            }
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $old = $params['old'] ?? null;
            $new = $params['new'] ?? null;
            $confirm = $params['confirm'] ?? null;

            if (!$old || !$new || !$confirm) {
                return new Response(400, ['error' => 'All password fields required']);
            }

            if ($new !== $confirm) {
                return new Response(400, ['error' => 'New password and confirm do not match']);
            }

            $user = UsersQuery::create()->findOneById($userId);
            if (!$user) {
                return new Response(404, ['error' => 'User not found']);
            }

            if (!password_verify($old, $user->getPassword())) {
                return new Response(400, ['error' => 'Wrong old password']);
            }

            $user->setPassword(password_hash($new, PASSWORD_DEFAULT));
            $user->save();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Удаление профиля по ID (только свой)
     */
    #[OA\Delete(
        path: '/api/users/{id}',
        tags: ['Users'],
        summary: 'Удаление профиля по ID (только для самого пользователя)',
        security: [['cookieAuth' => []]],
        parameters: [
            new OA\Parameter(
                name: 'id',
                in: 'path',
                required: true,
                schema: new OA\Schema(type: 'integer'),
                description: 'ID пользователя'
            )
        ],
        responses: [
            new OA\Response(
                response: 200,
                description: 'Профиль удалён',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'result', type: 'string', example: 'successful')
                    ]
                )
            ),
            new OA\Response(
                response: 400,
                description: 'ID не передан',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 401,
                description: 'Не авторизован',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 403,
                description: 'Попытка удалить чужой профиль',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            ),
            new OA\Response(
                response: 404,
                description: 'Пользователь не найден',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'error', type: 'string')
                    ]
                )
            )
        ]
    )]
    #[Route("/{id}", "DELETE")]
    public function deleteByID($params)
    {
        try {
            $id = $params['id'] ?? null;
            if (!$id) {
                return new Response(400, ['error' => 'User ID required']);
            }

            if($id == -1){
                $id = $_SESSION['id'];
            }

            if (($_SESSION['id'] ?? null) != $id) {
                return new Response(403, ['error' => 'You can only delete your own profile']);
            }

            return $this->innerDelete($id);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Выход из системы
     */
    #[OA\Post(
        path: '/api/users/logout',
        tags: ['Users'],
        summary: 'Выход из системы (удаление сессии и cookie)',
        responses: [
            new OA\Response(
                response: 200,
                description: 'Успешный выход',
                content: new OA\JsonContent(
                    properties: [
                        new OA\Property(property: 'success', type: 'boolean', example: true)
                    ]
                )
            )
        ]
    )]
    #[Route("/logout", "POST", false)]
    public function logout()
    {
        session_start();
        session_destroy();
        $_SESSION = [];

        $response = new Response(200, ['success' => true]);
        $response->setCook('jwt', '', time() - 3600, '/', '', true, true, 'Strict');

        return $response;
    }

    /**
     * Внутренний метод удаления пользователя
     */
    private function innerDelete($userId)
    {
        $user = UsersQuery::create()->findOneById($userId);
        if (!$user) {
            return new Response(404, ['error' => 'User not found']);
        }

        $user->delete();

        // Завершаем сессию и удаляем куку
        session_destroy();
        $_SESSION = [];
        $response = new Response(200, ['result' => 'successful']);
        $response->setCook('jwt', '', time() - 3600, '/', '', true, true, 'Strict');

        return $response;
    }

    /**
     * Форматирование полного профиля пользователя (без пароля)
     */
    private function formatUserProfile(Users $user): array
    {
        return [
            'Id' => $user->getId(),
            'FullName' => $user->getFullName(),
            'Phone' => $user->getPhone(),
            'Email' => $user->getEmail(),
            'City' => $user->getCity(),
            'BirthDate' => $user->getBirthDate() ? $user->getBirthDate()->format('Y-m-d') : null,
            'About' => $user->getAbout(),
            'ProfilePicture' => $user->getProfilePicture(),
            'vk_id' => $user->getVkId(),
            'tg_id' => $user->getTgId(),
            'Balance' => $user->getBalance(),
            'AvailabilityStatus' => $user->getAvailabilityStatus(),
            'IsAdmin' => $user->getIsAdmin(),
            'IsModerator' => $user->getIsModerator(),
            'LastSeenAt' => $user->getLastSeenAt() ? $user->getLastSeenAt()->format('Y-m-d H:i:s') : null,
            'IsOnline' => $user->getIsOnline(),
            'AverageRating' => $user->getAverageRating(),
            'CreatedAt' => $user->getCreatedAt() ? $user->getCreatedAt()->format('Y-m-d H:i:s') : null,
            'UpdatedAt' => $user->getUpdatedAt() ? $user->getUpdatedAt()->format('Y-m-d H:i:s') : null,
            'EducationDegree' => $user->getEducationDegree(),
            'EducationEndYear' => $user->getEducationEndYear(),
            'EducationStartYear' => $user->getEducationStartYear(),
            'EducationField' => $user->getEducationField(),
            'EducationInstitution' => $user->getEducationInstitution(),
        ];
    }
}