<?php
namespace Api;

use Classes\Validate;
use Core\Response;
use Core\Route;
use DateTime;
use Exception;
use Models\Chats;
use Models\ChatsQuery;
use Models\ChatMembers;
use Models\ChatMembersQuery;
use Models\Messages;
use Models\MessagesQuery;
use Models\UsersQuery;
use OpenApi\Attributes as OA;
use Propel\Runtime\ActiveQuery\Criteria;

#[OA\Tag(name: 'Chats', description: 'Управление чатами и сообщениями')]
#[Route("/api/chats")]
class ChatsController
{
    /**
     * Получить список чатов текущего пользователя (личные и групповые)
     */
    #[OA\Get(
        path: '/api/chats',
        tags: ['Chats'],
        summary: 'Список чатов пользователя',
        parameters: [
            new OA\Parameter(name: 'page', in: 'query', schema: new OA\Schema(type: 'integer', default: 1)),
            new OA\Parameter(name: 'limit', in: 'query', schema: new OA\Schema(type: 'integer', default: 20))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Список чатов', content: new OA\JsonContent(
                properties: [
                    new OA\Property(property: 'data', type: 'array', items: new OA\Items(ref: '#/components/schemas/Chat')),
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
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $page = max(1, (int)($params['page'] ?? 1));
            $limit = max(1, min(100, (int)($params['limit'] ?? 20)));
            $offset = ($page - 1) * $limit;

            // Получаем ID чатов, в которых состоит пользователь
            $chatIds = ChatMembersQuery::create()
                ->filterByUserId($userId)
                ->select('chat_id')
                ->find()
                ->toArray();

            if (empty($chatIds)) {
                return new Response(200, ['data' => [], 'total' => 0, 'page' => $page, 'limit' => $limit, 'pages' => 0]);
            }

            $query = ChatsQuery::create()
                ->filterById($chatIds, Criteria::IN)
                ->orderByCreatedAt('desc');

            $total = $query->count();
            $chats = $query->limit($limit)
                ->offset($offset)
                ->find();

            $data = [];
            foreach ($chats as $chat) {
                $data[] = $this->formatChat($chat, $userId);
            }

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
     * Получить детали чата (участники, сообщения с пагинацией)
     */
    #[OA\Get(
        path: '/api/chats/{id}',
        tags: ['Chats'],
        summary: 'Детальная информация о чате',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer')),
            new OA\Parameter(name: 'page', in: 'query', schema: new OA\Schema(type: 'integer', default: 1)),
            new OA\Parameter(name: 'limit', in: 'query', schema: new OA\Schema(type: 'integer', default: 50))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Данные чата', content: new OA\JsonContent(ref: '#/components/schemas/ChatDetail')),
            new OA\Response(response: 403, description: 'Нет доступа к чату'),
            new OA\Response(response: 404, description: 'Чат не найден'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}", "GET")]
    public function getById($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $chatId = $params['id'] ?? null;
            if (!$chatId) {
                return new Response(400, ['error' => 'Chat ID required']);
            }

            $chat = ChatsQuery::create()->findOneById($chatId);
            if (!$chat) {
                return new Response(404, ['error' => 'Chat not found']);
            }

            // Проверяем, является ли пользователь участником
            $member = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->filterByUserId($userId)
                ->findOne();
            if (!$member) {
                return new Response(403, ['error' => 'Access denied']);
            }

            $page = max(1, (int)($params['page'] ?? 1));
            $limit = max(1, min(100, (int)($params['limit'] ?? 50)));
            $offset = ($page - 1) * $limit;

            // Получаем сообщения с пагинацией
            $messagesQuery = MessagesQuery::create()
                ->filterByChatId($chatId)
                ->orderByCreatedAt('desc');
            $totalMessages = $messagesQuery->count();
            $messages = $messagesQuery->limit($limit)
                ->offset($offset)
                ->find();

            $formattedMessages = [];
            foreach ($messages as $msg) {
                $formattedMessages[] = $this->formatMessage($msg);
            }

            // Участники чата
            $members = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->joinWithUsers()
                ->find();
            $participants = [];
            foreach ($members as $m) {
                $user = $m->getUsers();
                $participants[] = [
                    'Id' => $user->getId(),
                    'FullName' => $user->getFullName(),
                    'ProfilePicture' => $user->getProfilePicture(),
                    'JoinedAt' => $m->getJoinedAt() ? $m->getJoinedAt()->format('Y-m-d H:i:s') : null,
                    'LastReadMessageId' => $m->getLastReadMessageId()
                ];
            }

            return new Response(200, [
                'chat' => $this->formatChat($chat, $userId),
                'participants' => $participants,
                'messages' => [
                    'data' => $formattedMessages,
                    'total' => $totalMessages,
                    'page' => $page,
                    'limit' => $limit,
                    'pages' => ceil($totalMessages / $limit)
                ]
            ]);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Создать новый чат (личный или групповой)
     */
    #[OA\Post(
        path: '/api/chats',
        tags: ['Chats'],
        summary: 'Создать чат',
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                required: ['type'],
                properties: [
                    new OA\Property(property: 'type', type: 'string', enum: ['personal', 'group'], description: 'Тип чата'),
                    new OA\Property(property: 'name', type: 'string', description: 'Название группового чата (обязательно для group)'),
                    new OA\Property(property: 'user_ids', type: 'array', items: new OA\Items(type: 'integer'), description: 'ID участников (для personal: ровно один другой пользователь; для group: не менее двух)')
                ]
            )
        ),
        responses: [
            new OA\Response(response: 200, description: 'Чат создан', content: new OA\JsonContent(
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

            $type = $params['type'] ?? null;
            if (!in_array($type, ['personal', 'group'])) {
                return new Response(400, ['error' => 'Invalid chat type']);
            }

            $userIds = isset($params['user_ids']) && is_array($params['user_ids']) ? $params['user_ids'] : [];
            $currentUserIncluded = in_array($userId, $userIds);
            if (!$currentUserIncluded) {
                $userIds[] = $userId;
            }

            // Проверяем, что все пользователи существуют
            $existingUsers = UsersQuery::create()
                ->filterById($userIds, Criteria::IN)
                ->count();
            if ($existingUsers != count($userIds)) {
                return new Response(400, ['error' => 'One or more users do not exist']);
            }

            $name = null; // для группового чата

            if ($type === 'personal') {
                if (count($userIds) != 2) {
                    return new Response(400, ['error' => 'Personal chat must have exactly 2 participants']);
                }
                $otherUserId = array_diff($userIds, [$userId])[0];

                $existingChat = ChatsQuery::create()
                    ->filterByType('personal')
                    ->where('EXISTS (SELECT 1 FROM chat_members cm1 WHERE cm1.chat_id = chats.id AND cm1.user_id = ?)', $userId)
                    ->where('EXISTS (SELECT 1 FROM chat_members cm2 WHERE cm2.chat_id = chats.id AND cm2.user_id = ?)', $otherUserId)
                    ->findOne();

                if ($existingChat) {
                    return new Response(409, ['id' => $existingChat->getId(), 'result' => 'already_exists']);
                }
            } elseif ($type === 'group') {
                $name = trim($params['name'] ?? '');
                if (empty($name)) {
                    return new Response(400, ['error' => 'Group chat name is required']);
                }
                if (count($userIds) < 2) {
                    return new Response(400, ['error' => 'Group chat must have at least 2 participants']);
                }
            }

            // Создаём чат
            $chat = new Chats();
            $chat->setType($type);
            if ($type === 'group') {
                $chat->setName($name);
            }
            $chat->save();

            // Добавляем участников
            foreach ($userIds as $uid) {
                $member = new ChatMembers();
                $member->setChatId($chat->getId());
                $member->setUserId($uid);
                $member->save();
            }

            return new Response(200, ['id' => $chat->getId(), 'result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Добавить участника в групповой чат
     */
    #[OA\Post(
        path: '/api/chats/{id}/members',
        tags: ['Chats'],
        summary: 'Добавить участника в групповой чат',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                required: ['user_id'],
                properties: [
                    new OA\Property(property: 'user_id', type: 'integer')
                ]
            )
        ),
        responses: [
            new OA\Response(response: 200, description: 'Участник добавлен'),
            new OA\Response(response: 400, description: 'Ошибка валидации'),
            new OA\Response(response: 403, description: 'Нет прав (только участники чата)'),
            new OA\Response(response: 404, description: 'Чат не найден'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}/members", "POST")]
    public function addMember($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $chatId = $params['id'] ?? null;
            if (!$chatId) {
                return new Response(400, ['error' => 'Chat ID required']);
            }

            $chat = ChatsQuery::create()->findOneById($chatId);
            if (!$chat) {
                return new Response(404, ['error' => 'Chat not found']);
            }
            if ($chat->getType() !== 'group') {
                return new Response(400, ['error' => 'Only group chats can have members added']);
            }

            // Проверяем, что текущий пользователь является участником чата
            $isMember = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->filterByUserId($userId)
                ->exists();
            if (!$isMember) {
                return new Response(403, ['error' => 'You are not a member of this chat']);
            }

            $newUserId = $params['user_id'] ?? null;
            if (!$newUserId) {
                return new Response(400, ['error' => 'User ID required']);
            }

            // Проверяем, существует ли пользователь
            $newUser = UsersQuery::create()->findOneById($newUserId);
            if (!$newUser) {
                return new Response(404, ['error' => 'User not found']);
            }

            // Проверяем, не состоит ли уже
            $existing = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->filterByUserId($newUserId)
                ->exists();
            if ($existing) {
                return new Response(400, ['error' => 'User already in chat']);
            }

            $member = new ChatMembers();
            $member->setChatId($chatId);
            $member->setUserId($newUserId);
            $member->save();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Удалить участника из группового чата
     */
    #[OA\Delete(
        path: '/api/chats/{id}/members/{userId}',
        tags: ['Chats'],
        summary: 'Удалить участника из группового чата',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer')),
            new OA\Parameter(name: 'userId', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Участник удалён'),
            new OA\Response(response: 403, description: 'Нет прав'),
            new OA\Response(response: 404, description: 'Чат или участник не найдены'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}/members/{user}", "DELETE")]
    public function removeMember($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $chatId = $params['id'] ?? null;
            $targetUserId = $params['user'] ?? null;
            if (!$chatId || !$targetUserId) {
                return new Response(400, ['error' => 'Chat ID and User ID required']);
            }

            $chat = ChatsQuery::create()->findOneById($chatId);
            if (!$chat) {
                return new Response(404, ['error' => 'Chat not found']);
            }
            if ($chat->getType() !== 'group') {
                return new Response(400, ['error' => 'Only group chats can have members removed']);
            }

            // Проверяем, что текущий пользователь является участником
            $isMember = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->filterByUserId($userId)
                ->exists();
            if (!$isMember) {
                return new Response(403, ['error' => 'You are not a member of this chat']);
            }

            // Находим запись участника
            $member = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->filterByUserId($targetUserId)
                ->findOne();
            if (!$member) {
                return new Response(404, ['error' => 'User is not a member of this chat']);
            }

            // Нельзя удалить последнего участника
            $membersCount = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->count();
            if ($membersCount <= 1) {
                return new Response(400, ['error' => 'Cannot remove the last participant']);
            }

            $member->delete();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Отправить сообщение в чат
     */
    #[OA\Post(
        path: '/api/chats/{id}/messages',
        tags: ['Chats'],
        summary: 'Отправить сообщение',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                required: ['content_type'],
                properties: [
                    new OA\Property(property: 'content_type', type: 'string', enum: ['text', 'image', 'file', 'voice']),
                    new OA\Property(property: 'content', type: 'string', description: 'Текст сообщения или описание файла'),
                    new OA\Property(property: 'file_url', type: 'string', description: 'URL файла (для image/file/voice)'),
                    new OA\Property(property: 'transcribed_text', type: 'string', description: 'Распознанный текст голосового сообщения')
                ]
            )
        ),
        responses: [
            new OA\Response(response: 200, description: 'Сообщение отправлено', content: new OA\JsonContent(
                properties: [
                    new OA\Property(property: 'id', type: 'integer'),
                    new OA\Property(property: 'result', type: 'string', example: 'successful')
                ]
            )),
            new OA\Response(response: 403, description: 'Не участник чата'),
            new OA\Response(response: 404, description: 'Чат не найден'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}/messages", "POST")]
    public function sendMessage($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $chatId = $params['id'] ?? null;
            if (!$chatId) {
                return new Response(400, ['error' => 'Chat ID required']);
            }

            $chat = ChatsQuery::create()->findOneById($chatId);
            if (!$chat) {
                return new Response(404, ['error' => 'Chat not found']);
            }

            // Проверяем, является ли пользователь участником
            $isMember = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->filterByUserId($userId)
                ->exists();
            if (!$isMember) {
                return new Response(403, ['error' => 'You are not a member of this chat']);
            }

            $contentType = $params['content_type'] ?? null;
            if (!in_array($contentType, ['text', 'image', 'file', 'voice'])) {
                return new Response(400, ['error' => 'Invalid content_type']);
            }

            $content = isset($params['content']) ? trim($params['content']) : null;
            $fileUrl = isset($params['file_url']) ? trim($params['file_url']) : null;
            $transcribedText = isset($params['transcribed_text']) ? trim($params['transcribed_text']) : null;

            if ($contentType === 'text' && empty($content)) {
                return new Response(400, ['error' => 'Text content is required for text messages']);
            }
            if (in_array($contentType, ['image', 'file', 'voice']) && empty($fileUrl)) {
                return new Response(400, ['error' => 'file_url is required for this content type']);
            }

            $message = new Messages();
            $message->setChatId($chatId);
            $message->setSenderId($userId);
            $message->setContentType($contentType);
            $message->setContent($content);
            $message->setFileUrl($fileUrl);
            $message->setTranscribedText($transcribedText);
            $message->setReply($params['reply'] ?? null);
            $message->setCreatedAt(new DateTime());
            $message->save();

            return new Response(200, ['data' => $this->formatMessage($message)]);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Получить сообщения чата (с пагинацией) – дублирует часть getById, но может быть полезно отдельно
     */
    #[OA\Get(
        path: '/api/chats/{id}/messages',
        tags: ['Chats'],
        summary: 'Получить сообщения чата',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer')),
            new OA\Parameter(name: 'page', in: 'query', schema: new OA\Schema(type: 'integer', default: 1)),
            new OA\Parameter(name: 'limit', in: 'query', schema: new OA\Schema(type: 'integer', default: 50))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Список сообщений'),
            new OA\Response(response: 403, description: 'Нет доступа'),
            new OA\Response(response: 404, description: 'Чат не найден'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}/messages", "GET")]
    public function getMessages($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $chatId = $params['id'] ?? null;
            if (!$chatId) {
                return new Response(400, ['error' => 'Chat ID required']);
            }

            $chat = ChatsQuery::create()->findOneById($chatId);
            if (!$chat) {
                return new Response(404, ['error' => 'Chat not found']);
            }

            $isMember = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->filterByUserId($userId)
                ->exists();
            if (!$isMember) {
                return new Response(403, ['error' => 'Access denied']);
            }

            $page = max(1, (int)($params['page'] ?? 1));
            $limit = max(1, min(100, (int)($params['limit'] ?? 50)));
            $offset = ($page - 1) * $limit;

            $query = MessagesQuery::create()
                ->filterByChatId($chatId)
                ->orderByCreatedAt('desc');
            $total = $query->count();
            $messages = $query->limit($limit)
                ->offset($offset)
                ->find();

            $data = [];
            foreach ($messages as $msg) {
                $data[] = $this->formatMessage($msg);
            }

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
     * Отметить сообщение как прочитанное (обновить last_read_message_id для текущего пользователя в чате)
     */
    #[OA\Put(
        path: '/api/chats/{id}/read',
        tags: ['Chats'],
        summary: 'Отметить сообщения как прочитанные до указанного ID',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer')),
            new OA\Parameter(name: 'message_id', in: 'query', required: true, schema: new OA\Schema(type: 'integer'), description: 'ID последнего прочитанного сообщения')
        ],
        responses: [
            new OA\Response(response: 200, description: 'Статус обновлён'),
            new OA\Response(response: 403, description: 'Нет доступа'),
            new OA\Response(response: 404, description: 'Чат или сообщение не найдены'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/{id}/read", "PUT")]
    public function markRead($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $chatId = $params['id'] ?? null;
            $messageId = $params['message_id'] ?? null;
            if (!$chatId || !$messageId) {
                return new Response(400, ['error' => 'Chat ID and message_id required']);
            }

            // Проверяем существование сообщения и что оно в этом чате
            $message = MessagesQuery::create()
                ->filterById($messageId)
                ->filterByChatId($chatId)
                ->findOne();
            if (!$message) {
                return new Response(404, ['error' => 'Message not found in this chat']);
            }

            $member = ChatMembersQuery::create()
                ->filterByChatId($chatId)
                ->filterByUserId($userId)
                ->findOne();
            if (!$member) {
                return new Response(403, ['error' => 'You are not a member of this chat']);
            }

            $member->setLastReadMessageId($messageId);
            $member->save();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Редактировать текст сообщения (только своё)
     */
    #[OA\Put(
        path: '/api/messages/{id}',
        tags: ['Chats'],
        summary: 'Редактировать сообщение',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        requestBody: new OA\RequestBody(
            required: true,
            content: new OA\JsonContent(
                required: ['content'],
                properties: [
                    new OA\Property(property: 'content', type: 'string')
                ]
            )
        ),
        responses: [
            new OA\Response(response: 200, description: 'Сообщение обновлено'),
            new OA\Response(response: 403, description: 'Не автор'),
            new OA\Response(response: 404, description: 'Сообщение не найдено'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/messages/{id}", "PUT")]
    public function editMessage($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $messageId = $params['id'] ?? null;
            if (!$messageId) {
                return new Response(400, ['error' => 'Message ID required']);
            }

            $message = MessagesQuery::create()->findOneById($messageId);
            if (!$message) {
                return new Response(404, ['error' => 'Message not found']);
            }

            if ($message->getSenderId() != $userId) {
                return new Response(403, ['error' => 'You can only edit your own messages']);
            }

            $newContent = trim($params['content'] ?? '');
            if (empty($newContent)) {
                return new Response(400, ['error' => 'Content cannot be empty']);
            }

            $message->setContent($newContent);
            $message->save();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    /**
     * Удалить своё сообщение
     */
    #[OA\Delete(
        path: '/api/messages/{id}',
        tags: ['Chats'],
        summary: 'Удалить сообщение',
        parameters: [
            new OA\Parameter(name: 'id', in: 'path', required: true, schema: new OA\Schema(type: 'integer'))
        ],
        responses: [
            new OA\Response(response: 200, description: 'Сообщение удалено'),
            new OA\Response(response: 403, description: 'Не автор'),
            new OA\Response(response: 404, description: 'Сообщение не найдено'),
            new OA\Response(response: 401, description: 'Не авторизован')
        ]
    )]
    #[Route("/messages/{id}", "DELETE")]
    public function deleteMessage($params)
    {
        try {
            $userId = $_SESSION['id'] ?? null;
            if (!$userId) {
                return new Response(401, ['error' => 'Unauthorized']);
            }

            $messageId = $params['id'] ?? null;
            if (!$messageId) {
                return new Response(400, ['error' => 'Message ID required']);
            }

            $message = MessagesQuery::create()->findOneById($messageId);
            if (!$message) {
                return new Response(404, ['error' => 'Message not found']);
            }

            if ($message->getSenderId() != $userId) {
                return new Response(403, ['error' => 'You can only delete your own messages']);
            }

            $message->delete();

            return new Response(200, ['result' => 'successful']);
        } catch (Exception $ex) {
            return Validate::Ex($ex);
        }
    }

    // ========== Форматирование ==========

    private function formatChat(Chats $chat, int $currentUserId): array
    {
        $data = [
            'Id' => $chat->getId(),
            'Type' => $chat->getType(),
            'Name' => $chat->getName(),
            'CreatedAt' => $chat->getCreatedAt() ? $chat->getCreatedAt()->format('Y-m-d H:i:s') : null,
        ];

        // Для личных чатов добавим информацию о собеседнике
        if ($chat->getType() === 'personal') {
            $otherMember = ChatMembersQuery::create()
                ->filterByChatId($chat->getId())
                ->filterByUserId($currentUserId, Criteria::NOT_EQUAL)
                ->joinWithUsers()
                ->findOne();
            if ($otherMember) {
                $user = $otherMember->getUsers();
                $data['OtherUser'] = [
                    'Id' => $user->getId(),
                    'FullName' => $user->getFullName(),
                    'ProfilePicture' => $user->getProfilePicture(),
                    'IsOnline' => $user->getIsOnline(),
                    'LastSeenAt' => $user->getLastSeenAt() ? $user->getLastSeenAt()->format('Y-m-d H:i:s') : null,
                ];
            }
        }

        // Последнее сообщение (опционально)
        $lastMessage = MessagesQuery::create()
            ->filterByChatId($chat->getId())
            ->orderByCreatedAt('desc')
            ->findOne();
        if ($lastMessage) {
            $data['LastMessage'] = $this->formatMessage($lastMessage);
        }

        // Непрочитанные сообщения (где id > last_read_message_id текущего пользователя)
        $member = ChatMembersQuery::create()
            ->filterByChatId($chat->getId())
            ->filterByUserId($currentUserId)
            ->findOne();
        $lastReadId = $member ? $member->getLastReadMessageId() : null;
        $unreadCount = 0;

        if ($lastReadId === null) {
            // Если нет записи о прочитанном сообщении, все сообщения считаются непрочитанными
            $unreadCount = MessagesQuery::create()
                ->filterByChatId($chat->getId())
                ->count();
        } else {
            $unreadCount = MessagesQuery::create()
                ->filterByChatId($chat->getId())
                ->filterById($lastReadId, Criteria::GREATER_THAN)
                ->count();
        }
        $data['UnreadCount'] = $unreadCount;

        return $data;
    }

    private function formatMessage(Messages $message, bool $CheckReply = true): array
    {
        $sender = UsersQuery::create()->findOneById($message->getSenderId());
        $data = [
            'Id' => $message->getId(),
            'SenderId' => $message->getSenderId(),
            'SenderName' => $sender ? $sender->getFullName() : null,
            'SenderProfilePicture' => $sender ? $sender->getProfilePicture() : null,
            'ContentType' => $message->getContentType(),
            'Content' => $message->getContent(),
            'FileUrl' => $message->getFileUrl(),
            'TranscribedText' => $message->getTranscribedText(),
            'CreatedAt' => $message->getCreatedAt() ? $message->getCreatedAt()->format('Y-m-d H:i:s') : null
        ];
        if ($CheckReply) {
            $data['reply'] = $message->getReply() ? $this->formatMessage(MessagesQuery::create()->findOneById($message->getReply()), false) : null;
            return $data;
        }
        else {
            return $data;
        }
    }
}