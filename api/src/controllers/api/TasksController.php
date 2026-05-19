<?php
namespace Api;

use Classes\Validate;
use Core\Response;
use Core\Route;
use Exception;
use Models\Tasks;
use Models\TasksQuery;
use Models\TaskVolunteers;
use Models\TaskVolunteersQuery;
use Models\UsersQuery;
use OpenApi\Attributes as OA;

#[OA\Tag(name: 'Tasks', description: 'Управление задачами')]
#[Route("/api/tasks")]
class TasksController
{

    #[OA\Get(
        path: '/api/tasks/{taskId}/my-status',
        tags: ['Tasks'],
        summary: 'Получить статус отклика текущего пользователя на задачу',
        parameters: [
            new OA\Parameter(name: 'taskId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Статус', content: new OA\JsonContent(
                properties: [
                    new OA\Property(property: 'status', type: 'string', enum: ['pending', 'accepted', 'rejected', null])
                ]
            )),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{task}/my-status", "GET")]
    public function myVolunteerStatus($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }
    
            $taskId = $params['task'] ?? null;
            if (!$taskId) {
                return new Response(400, ['error' => 'Task ID required']);
            }
    
            $volunteer = TaskVolunteersQuery::create()
                ->filterByTaskId($taskId)
                ->filterByUserId($userId)
                ->findOne();
    
            $status = $volunteer ? $volunteer->getStatus() : null;
            return new Response(200, ['status' => $status]);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Откликнуться на задачу (для волонтёра)
     */
    #[OA\Post(
        path: '/api/tasks/{taskId}/volunteer',
        tags: ['Tasks'],
        summary: 'Откликнуться на задачу',
        parameters: [
            new OA\Parameter(name: 'taskId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Отклик успешно создан'),
            new OA\Response(response: 400, description: 'Ошибка (уже откликался, задача не в поиске и т.д.)'),
            new OA\Response(response: 401, description: 'Не авторизован'),
            new OA\Response(response: 404, description: 'Задача не найдена')
        ]
    )]
    #[Route("/{task}/volunteer", "POST")]
    public function volunteer($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $taskId = $params['task'] ?? null;
            if (!$taskId) {
                return new Response(400, ['error' => 'Task ID required']);
            }

            $task = TasksQuery::create()->findOneById($taskId);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            if ($task->getStatus() !== 'searching') {
                return new Response(400, ['error' => 'Task is not accepting volunteers']);
            }

            // Проверяем, не откликался ли уже
            $existing = TaskVolunteersQuery::create()
                ->filterByTaskId($taskId)
                ->filterByUserId($userId)
                ->findOne();
            if ($existing) {
                return new Response(400, ['error' => 'You already responded to this task']);
            }

            $volunteer = new TaskVolunteers();
            $volunteer->setTaskId($taskId);
            $volunteer->setUserId($userId);
            $volunteer->setStatus('pending');
            $volunteer->save();

            return new Response(200, ['result' => 'successful', 'volunteer_id' => $volunteer->getId()]);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Получить список кандидатов (для автора задачи)
     */
    #[OA\Get(
        path: '/api/tasks/{taskId}/volunteers',
        tags: ['Tasks'],
        summary: 'Список откликнувшихся волонтёров',
        parameters: [
            new OA\Parameter(name: 'taskId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Список кандидатов', content: new OA\JsonContent(
                type: 'array',
                items: new OA\Items(
                    properties: [
                        new OA\Property(property: 'id', type: 'integer'),
                        new OA\Property(property: 'full_name', type: 'string'),
                        new OA\Property(property: 'birth_date', type: 'string', format: 'date', nullable: true),
                        new OA\Property(property: 'profile_picture', type: 'string', nullable: true),
                        new OA\Property(property: 'status', type: 'string', enum: ['pending', 'accepted', 'rejected'])
                    ]
                )
            )),
            new OA\Response(response: 401, description: 'Не авторизован'),
            new OA\Response(response: 403, description: 'Нет прав (только автор задачи)'),
            new OA\Response(response: 404, description: 'Задача не найдена')
        ]
    )]
    #[Route("/{task}/volunteers", "GET")]
    public function getVolunteers($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $taskId = $params['task'] ?? null;
            if (!$taskId) {
                return new Response(400, ['error' => 'Task ID required']);
            }

            $task = TasksQuery::create()->findOneById($taskId);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            if ($task->getCreatedByUserId() != $userId) {
                return new Response(403, ['error' => 'Only task creator can view volunteers']);
            }
            
            $volunteers = TaskVolunteersQuery::create()->filterByTaskId($taskId)->joinUsers()->find();

            $result = [];
            foreach ($volunteers as $v) {
                $user = $v->getUsers();
                $result[] = [
                    'Id' => $user->getId(),
                    'FullName' => $user->getFullName(),
                    'BirthDate' => $user->getBirthDate() ? $user->getBirthDate()->format('Y-m-d') : null,
                    'ProfilePicture' => $user->getProfilePicture(),
                    'Status' => $v->getStatus()
                ];
            }

            return new Response(200, $result);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Принять волонтёра
     */
    #[OA\Put(
        path: '/api/tasks/{taskId}/volunteers/{userId}/accept',
        tags: ['Tasks'],
        summary: 'Принять волонтёра',
        parameters: [
            new OA\Parameter(name: 'taskId', in: 'path', required: true, schema: new OA\Schema(type: 'integer')),
            new OA\Parameter(name: 'userId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Принят'),
            new OA\Response(response: 400, description: 'Ошибка (задача не в поиске, лимит и т.д.)'),
            new OA\Response(response: 401, description: 'Не авторизован'),
            new OA\Response(response: 403, description: 'Нет прав'),
            new OA\Response(response: 404, description: 'Задача или отклик не найдены')
        ]
    )]
    #[Route("/{task}/volunteers/{user}/accept", "PUT")]
    public function acceptVolunteer($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $taskId = $params['task'] ?? null;
            $volunteerUserId = $params['user'] ?? null;
            if (!$taskId || !$volunteerUserId) {
                return new Response(400, ['error' => 'Task ID and User ID required']);
            }

            $task = TasksQuery::create()->findOneById($taskId);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            if ($task->getCreatedByUserId() != $userId) {
                return new Response(403, ['error' => 'Only task creator can accept volunteers']);
            }

            if ($task->getStatus() !== 'searching') {
                return new Response(400, ['error' => 'Task is not in searching status']);
            }

            $volunteer = TaskVolunteersQuery::create()
                ->filterByTaskId($taskId)
                ->filterByUserId($volunteerUserId)
                ->findOne();
            if (!$volunteer) {
                return new Response(404, ['error' => 'Volunteer response not found']);
            }

            if ($volunteer->getStatus() !== 'pending') {
                return new Response(400, ['error' => 'Volunteer already processed']);
            }

            // Проверяем лимит: сколько уже принято
            $acceptedCount = TaskVolunteersQuery::create()
                ->filterByTaskId($taskId)
                ->filterByStatus('accepted')
                ->count();
            if ($acceptedCount >= $task->getNeededVolunteers()) {
                return new Response(400, ['error' => 'Maximum volunteers reached']);
            }

            $volunteer->setStatus('accepted');
            $volunteer->save();

            // Если после принятия набрано нужное количество, можно автоматически перевести задачу в in_progress? 
            // По желанию можно сделать, но по логике автор сам решает начать выполнение. Оставим как есть.

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Отклонить волонтёра
     */
    #[OA\Put(
        path: '/api/tasks/{task}/volunteers/{user}/reject',
        tags: ['Tasks'],
        summary: 'Отклонить волонтёра',
        parameters: [
            new OA\Parameter(name: 'taskId', in: 'path', required: true, schema: new OA\Schema(type: 'integer')),
            new OA\Parameter(name: 'userId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Отклонён'),
            new OA\Response(response: 401, description: 'Не авторизован'),
            new OA\Response(response: 403, description: 'Нет прав'),
            new OA\Response(response: 404, description: 'Задача или отклик не найдены')
        ]
    )]
    #[Route("/{task}/volunteers/{user}/reject", "PUT")]
    public function rejectVolunteer($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $taskId = $params['task'] ?? null;
            $volunteerUserId = $params['user'] ?? null;
            if (!$taskId || !$volunteerUserId) {
                return new Response(400, ['error' => 'Task ID and User ID required']);
            }

            $task = TasksQuery::create()->findOneById($taskId);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            if ($task->getCreatedByUserId() != $userId) {
                return new Response(403, ['error' => 'Only task creator can reject volunteers']);
            }

            $volunteer = TaskVolunteersQuery::create()
                ->filterByTaskId($taskId)
                ->filterByUserId($volunteerUserId)
                ->findOne();
            if (!$volunteer) {
                return new Response(404, ['error' => 'Volunteer response not found']);
            }

            if ($volunteer->getStatus() !== 'pending') {
                return new Response(400, ['error' => 'Volunteer already processed']);
            }

            $volunteer->setStatus('rejected');
            $volunteer->save();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Начать выполнение задачи (автор)
     */
    #[OA\Put(
        path: '/api/tasks/{task}/start',
        tags: ['Tasks'],
        summary: 'Начать выполнение задачи',
        parameters: [
            new OA\Parameter(name: 'taskId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Статус изменён на in_progress'),
            new OA\Response(response: 400, description: 'Нет принятых волонтёров'),
            new OA\Response(response: 401, description: 'Не авторизован'),
            new OA\Response(response: 403, description: 'Нет прав'),
            new OA\Response(response: 404, description: 'Задача не найдена')
        ]
    )]
    #[Route("/{task}/start", "PUT")]
    public function startTask($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $taskId = $params['task'] ?? null;
            if (!$taskId) {
                return new Response(400, ['error' => 'Task ID required']);
            }

            $task = TasksQuery::create()->findOneById($taskId);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            if ($task->getCreatedByUserId() != $userId) {
                return new Response(403, ['error' => 'Only task creator can start the task']);
            }

            if ($task->getStatus() !== 'searching') {
                return new Response(400, ['error' => 'Task already started or completed']);
            }

            // Проверяем, есть ли хотя бы один принятый волонтёр
            $acceptedCount = TaskVolunteersQuery::create()
                ->filterByTaskId($taskId)
                ->filterByStatus('accepted')
                ->count();
            if ($acceptedCount == 0) {
                return new Response(400, ['error' => 'No accepted volunteers yet']);
            }

            $task->setStatus('in_progress');
            $task->save();

            // Можно также отклонить всех оставшихся pending, но по желанию оставим их (они просто будут висеть)

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Завершить задачу (автор)
     */
    #[OA\Put(
        path: '/api/tasks/{taskId}/complete',
        tags: ['Tasks'],
        summary: 'Завершить задачу и начислить награду',
        parameters: [
            new OA\Parameter(name: 'taskId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Задача завершена, награда начислена'),
            new OA\Response(response: 401, description: 'Не авторизован'),
            new OA\Response(response: 403, description: 'Нет прав'),
            new OA\Response(response: 404, description: 'Задача не найдена')
        ]
    )]
    #[Route("/{task}/complete", "PUT")]
    public function completeTask($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $taskId = $params['task'] ?? null;
            if (!$taskId) {
                return new Response(400, ['error' => 'Task ID required']);
            }

            $task = TasksQuery::create()->findOneById($taskId);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            if ($task->getCreatedByUserId() != $userId) {
                return new Response(403, ['error' => 'Only task creator can complete the task']);
            }

            if ($task->getStatus() !== 'in_progress') {
                return new Response(400, ['error' => 'Task is not in progress']);
            }

            // Получаем всех принятых волонтёров
            $acceptedVolunteers = TaskVolunteersQuery::create()
                ->filterByTaskId($taskId)
                ->filterByStatus('accepted')
                ->find();

            $rewardPerVolunteer = $task->getReward() / $task->getNeededVolunteers(); // если награда общая, делим поровну
            // Или если награда за каждого волонтёра отдельно, то просто reward (если в задаче указана награда за одного волонтёра). 
            // В текущей структуре reward — это общая сумма? По смыслу — награда за задачу, делится между волонтёрами. Уточним.
            // Предположим, что reward — общая сумма, делим на количество принятых (или на нужное количество). 
            // Но если принято меньше, чем needed, то оставшиеся нирбики? Лучше делить между фактическими исполнителями.
            $actualCount = count($acceptedVolunteers);
            if ($actualCount == 0) {
                return new Response(400, ['error' => 'No accepted volunteers']);
            }
            $amountPerVolunteer = $task->getReward() / $actualCount;

            // Начисляем каждому
            foreach ($acceptedVolunteers as $vol) {
                $user = UsersQuery::create()->findOneById($vol->getUserId());
                if ($user) {
                    $oldBalance = $user->getBalance();
                    $newBalance = $oldBalance + $amountPerVolunteer;
                    $user->setBalance($newBalance);
                    $user->save();

                    // Запись транзакции
                    $transaction = new \Models\BalanceTransactions(); // предположим, есть такая модель
                    $transaction->setUserId($user->getId());
                    $transaction->setAmount($amountPerVolunteer);
                    $transaction->setBalanceAfter($newBalance);
                    $transaction->setType('task_reward');
                    $transaction->setReferenceType('task');
                    $transaction->setReferenceId($taskId);
                    $transaction->save();

                    // Обновляем статус волонтёра на completed? Можно
                    $vol->setStatus('completed');
                    $vol->save();
                }
            }

            $task->setStatus('completed');
            $task->save();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Получение задач конкретного пользователя
     */
    #[OA\Get(
        path: '/api/tasks/user/{id}',
        tags: ['Tasks'],
        summary: 'Получить задачи пользователя',
        parameters: [
            new OA\Parameter(name: 'userId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'), description: 'ID пользователя'),
            new OA\Parameter(name: 'status', in: 'query', required: false, schema: new OA\Schema(type: 'string', enum: ['searching', 'in_progress', 'completed', 'cancelled']), description: 'Фильтр по статусу'),
            new OA\Parameter(name: 'page', in: 'query', required: false, schema: new OA\Schema(type: 'integer', default: 1)),
            new OA\Parameter(name: 'limit', in: 'query', required: false, schema: new OA\Schema(type: 'integer', default: 10))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Список задач пользователя', content: new OA\JsonContent(
                properties: [
                    new OA\Property(property: 'data', type: 'array', items: new OA\Items(ref: '#/components/schemas/Task')),
                    new OA\Property(property: 'total', type: 'integer'),
                    new OA\Property(property: 'page', type: 'integer'),
                    new OA\Property(property: 'limit', type: 'integer'),
                    new OA\Property(property: 'pages', type: 'integer')
                ]
            )),
            new OA\Response(response: 401, description: 'Не авторизован'),
            new OA\Response(response: 404, description: 'Пользователь не найден')
        ]
    )]
    #[Route("/user/{id}", "GET")]
    public function getUserTasks($params)
    {
        try {
    
            $userId = $params['id'] ?? null;
            if (!$userId) {
                return new Response(400, ['error' => 'User ID required']);
            }
    
            $user = UsersQuery::create()->findOneById($userId);
            if (!$user) {
                return new Response(404, ['error' => 'User not found']);
            }
    
            $page = max(1, (int)($params['page'] ?? 1));
            $limit = max(1, min(100, (int)($params['limit'] ?? 10)));
            $offset = ($page - 1) * $limit;
    
            $query = TasksQuery::create()
                ->filterByCreatedByUserId($userId)
                ->orderByCreatedAt('desc');
    
            if (!empty($params['status'])) {
                $query->filterByStatus($params['status']);
            }
    
            $total = $query->count();
            $tasks = $query->limit($limit)
                ->offset($offset)
                ->find();
    
            $data = $this->formatTasks($tasks);
    
            return new Response(200, [
                'data' => $data,
                'total' => $total,
                'page' => $page,
                'limit' => $limit,
                'pages' => ceil($total / $limit)
            ]);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Создание новой задачи
     */
    #[OA\Post(
        path: '/api/tasks',
        tags: ['Tasks'],
        summary: 'Создать задачу',
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                required: ['title', 'needed_volunteers', 'deadline'],
                properties: [
                    new OA\Property(property: 'title', type: 'string', example: 'Помочь с переездом'),
                    new OA\Property(property: 'description', type: 'string', example: 'Нужны сильные руки'),
                    new OA\Property(property: 'needed_volunteers', type: 'integer', example: 2),
                    new OA\Property(property: 'priority', type: 'string', enum: ['low', 'medium', 'high'], example: 'medium'),
                    new OA\Property(property: 'location', type: 'string', example: 'ул. Ленина, 10'),
                    new OA\Property(property: 'reward', type: 'number', format: 'float', example: 100.50),
                    new OA\Property(property: 'deadline', type: 'string', format: 'date-time', example: '2025-12-31 23:59:59')
                ]
            )
        ),
        responses: [
            new OA\Response(response: 200, description: 'Задача создана', content: new OA\JsonContent(
                properties: [
                    new OA\Property(property: 'id', type: 'integer'),
                    new OA\Property(property: 'result', type: 'string', example: 'successful')
                ]
            )),
            new OA\Response(response: 400, description: 'Ошибка валидации'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("", "POST")]
    public function create($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $required = ['title', 'needed_volunteers', 'deadline'];
            if (Validate::checkParams($required, $params)) {
                return new Response(400, ['error' => 'Missing required fields']);
            }

            $title = trim($params['title']);
            $description = isset($params['description']) ? trim($params['description']) : null;
            $neededVolunteers = (int)$params['needed_volunteers'];
            $priority = $params['priority'] ?? 'medium';
            $location = isset($params['location']) ? trim($params['location']) : null;
            $reward = isset($params['reward']) ? (float)$params['reward'] : 0.0;
            $deadline = $params['deadline'];

            if ($neededVolunteers < 1) {
                return new Response(400, ['error' => 'needed_volunteers must be at least 1']);
            }
            $allowedPriorities = ['low', 'medium', 'high'];
            if (!in_array($priority, $allowedPriorities)) {
                return new Response(400, ['error' => 'Invalid priority']);
            }
            $deadlineObj = \DateTime::createFromFormat('Y-m-d H:i:s', $deadline);
            if (!$deadlineObj) {
                return new Response(400, ['error' => 'Invalid deadline format, expected YYYY-MM-DD HH:MM:SS']);
            }

            $task = new Tasks();
            $task->setTitle($title);
            $task->setDescription($description);
            $task->setNeededVolunteers($neededVolunteers);
            $task->setPriority($priority);
            $task->setLocation($location);
            $task->setReward($reward);
            $task->setStatus('searching');
            $task->setCreatedByUserId($userId);
            $task->setDeadline($deadlineObj);
            $task->save();

            return new Response(200, ['id' => $task->getId(), 'result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Получение списка задач с пагинацией
     */
    #[OA\Get(
        path: '/api/tasks',
        tags: ['Tasks'],
        summary: 'Получить список задач',
        parameters: [
            new OA\Parameter(name: 'page', in: 'query', required: false, schema: new OA\Schema(type: 'integer', default: 1), description: 'Номер страницы'),
            new OA\Parameter(name: 'limit', in: 'query', required: false, schema: new OA\Schema(type: 'integer', default: 10), description: 'Количество записей на странице'),
            new OA\Parameter(name: 'status', in: 'query', required: false, schema: new OA\Schema(type: 'string', enum: ['searching', 'in_progress', 'completed', 'cancelled']), description: 'Фильтр по статусу'),
            new OA\Parameter(name: 'priority', in: 'query', required: false, schema: new OA\Schema(type: 'string', enum: ['low', 'medium', 'high']), description: 'Фильтр по приоритету'),
            new OA\Parameter(name: 'city', in: 'query', required: false, schema: new OA\Schema(type: 'string'), description: 'Фильтр по городу (по полю location)')
        ],
        responses: [
            new OA\Response(response: 200, description: 'Список задач', content: new OA\JsonContent(
                properties: [
                    new OA\Property(property: 'data', type: 'array', items: new OA\Items(
                        properties: [
                            new OA\Property(property: 'id', type: 'integer'),
                            new OA\Property(property: 'title', type: 'string'),
                            new OA\Property(property: 'description', type: 'string', nullable: true),
                            new OA\Property(property: 'needed_volunteers', type: 'integer'),
                            new OA\Property(property: 'priority', type: 'string'),
                            new OA\Property(property: 'location', type: 'string', nullable: true),
                            new OA\Property(property: 'reward', type: 'number', format: 'float'),
                            new OA\Property(property: 'status', type: 'string'),
                            new OA\Property(property: 'creator_id', type: 'integer'),
                            new OA\Property(property: 'deadline', type: 'string', format: 'date-time'),
                            new OA\Property(property: 'created_at', type: 'string', format: 'date-time'),
                            new OA\Property(property: 'updated_at', type: 'string', format: 'date-time')
                        ]
                    )),
                    new OA\Property(property: 'total', type: 'integer'),
                    new OA\Property(property: 'page', type: 'integer'),
                    new OA\Property(property: 'limit', type: 'integer'),
                    new OA\Property(property: 'pages', type: 'integer')
                ]
            )),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("", "GET")]
    public function list($params)
    {
        try {
            
            if (!isset($_SESSION['id'])) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $page = max(1, (int)($params['page'] ?? 1));
            $limit = max(1, min(100, (int)($params['limit'] ?? 10))); // ограничим максимум 100
            $offset = ($page - 1) * $limit;

            $query = TasksQuery::create()
                ->orderByCreatedAt('desc');

            // Фильтры
            if (!empty($params['status'])) {
                $query->filterByStatus($params['status']);
            }
            if (!empty($params['priority'])) {
                $query->filterByPriority($params['priority']);
            }
            if (!empty($params['city'])) {
                $query->filterByLocation('%' . $params['city'] . '%', \Propel\Runtime\ActiveQuery\Criteria::LIKE);
            }

            $total = $query->count();
            $tasks = $query->limit($limit)
                ->offset($offset)
                ->find();

            $data = $this->formatTasks($tasks);

            return new Response(200, [
                'data' => $data,
                'total' => $total,
                'page' => $page,
                'limit' => $limit,
                'pages' => ceil($total / $limit)
            ]);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Получение одной задачи по ID
     */
    #[OA\Get(
        path: '/api/tasks/{id}',
        tags: ['Tasks'],
        summary: 'Получить задачу по ID',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Данные задачи'),
            new OA\Response(response: 404, description: 'Задача не найдена'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}", "GET")]
    public function getById($params)
    {
        try {
            
            if (!isset($_SESSION['id'])) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $id = $params['id'] ?? null;
            if (!$id) {
                return new Response(400, ['error' => 'ID required']);
            }

            $task = TasksQuery::create()->findOneById($id);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            $data = $this->formatTask($task);

            return new Response(200, $data);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Обновление задачи
     */
    #[OA\Put(
        path: '/api/tasks/{id}',
        tags: ['Tasks'],
        summary: 'Обновить задачу (только для создателя)',
        requestBody: new OA\RequestBody(
            content: new OA\JsonContent(
                properties: [
                    new OA\Property(property: 'title', type: 'string'),
                    new OA\Property(property: 'description', type: 'string', nullable: true),
                    new OA\Property(property: 'needed_volunteers', type: 'integer'),
                    new OA\Property(property: 'priority', type: 'string', enum: ['low', 'medium', 'high']),
                    new OA\Property(property: 'location', type: 'string', nullable: true),
                    new OA\Property(property: 'reward', type: 'number', format: 'float'),
                    new OA\Property(property: 'status', type: 'string', enum: ['searching', 'in_progress', 'completed', 'cancelled']),
                    new OA\Property(property: 'deadline', type: 'string', format: 'date-time')
                ]
            )
        ),
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Обновлено успешно'),
            new OA\Response(response: 403, description: 'Нет прав'),
            new OA\Response(response: 404, description: 'Задача не найдена'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}", "PUT")]
    public function update($params)
    {
        try {
            
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $id = $params['id'] ?? null;
            if (!$id) {
                return new Response(400, ['error' => 'ID required']);
            }

            $task = TasksQuery::create()->findOneById($id);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            if ($task->getCreatedByUserId() != $userId) {
                return new Response(403, ['error' => 'You can only edit your own tasks']);
            }

            // Обновляем поля, если они переданы
            if (isset($params['title'])) {
                $task->setTitle(trim($params['title']));
            }
            if (array_key_exists('description', $params)) { // можно передать null
                $task->setDescription(isset($params['description']) ? trim($params['description']) : null);
            }
            if (isset($params['needed_volunteers'])) {
                $val = (int)$params['needed_volunteers'];
                if ($val < 1) return new Response(400, ['error' => 'needed_volunteers must be at least 1']);
                $task->setNeededVolunteers($val);
            }
            if (isset($params['priority'])) {
                if (!in_array($params['priority'], ['low', 'medium', 'high'])) {
                    return new Response(400, ['error' => 'Invalid priority']);
                }
                $task->setPriority($params['priority']);
            }
            if (array_key_exists('location', $params)) {
                $task->setLocation(isset($params['location']) ? trim($params['location']) : null);
            }
            if (isset($params['reward'])) {
                $task->setReward((float)$params['reward']);
            }
            if (isset($params['status'])) {
                $allowed = ['searching', 'in_progress', 'completed', 'cancelled'];
                if (!in_array($params['status'], $allowed)) {
                    return new Response(400, ['error' => 'Invalid status']);
                }
                $task->setStatus($params['status']);
            }
            if (isset($params['deadline'])) {
                $deadlineObj = \DateTime::createFromFormat('Y-m-d H:i:s', $params['deadline']);
                if (!$deadlineObj) {
                    return new Response(400, ['error' => 'Invalid deadline format']);
                }
                $task->setDeadline($deadlineObj);
            }

            $task->save();
            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Удаление задачи
     */
    #[OA\Delete(
        path: '/api/tasks/{id}',
        tags: ['Tasks'],
        summary: 'Удалить задачу (только для создателя)',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Удалено успешно'),
            new OA\Response(response: 403, description: 'Нет прав'),
            new OA\Response(response: 404, description: 'Задача не найдена'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}", "DELETE")]
    public function delete($params)
    {
        try {
            
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $id = $params['id'] ?? null;
            if (!$id) {
                return new Response(400, ['error' => 'ID required']);
            }

            $task = TasksQuery::create()->findOneById($id);
            if (!$task) {
                return new Response(404, ['error' => 'Task not found']);
            }

            if ($task->getCreatedByUserId() != $userId) {
                return new Response(403, ['error' => 'You can only delete your own tasks']);
            }

            $task->delete();
            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    private function formatTask($task) {
        return [
                    'Id' => $task->getId(),
                    'Title' => $task->getTitle(),
                    'Description' => $task->getDescription(),
                    'NeededVolunteers' => $task->getNeededVolunteers(),
                    'Priority' => $task->getPriority(),
                    'Location' => $task->getLocation(),
                    'Reward' => $task->getReward(),
                    'Status' => $task->getStatus(),
                    'CreatorId' => $task->getCreatedByUserId(),
                    'CreatorFIO' => UsersQuery::create()->findOneById($task->getCreatedByUserId())->getFullName(),
                    'Deadline' => $task->getDeadline() ? $task->getDeadline()->format('Y-m-d H:i:s') : null,
                    'CreatedAt' => $task->getCreatedAt() ? $task->getCreatedAt()->format('Y-m-d H:i:s') : null,
                    'UpdatedAt' => $task->getUpdatedAt() ? $task->getUpdatedAt()->format('Y-m-d H:i:s') : null,
                ];
    }

    private function formatTasks($tasks) {
        $data = [];
        foreach ($tasks as $task) {
                $data[] = $this->formatTask($task);
            }
        return $data;
    }
}