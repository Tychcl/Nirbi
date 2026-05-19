-- phpMyAdmin SQL Dump
-- version 5.2.3
-- https://www.phpmyadmin.net/
--
-- Хост: db:3306
-- Время создания: Май 11 2026 г., 15:07
-- Версия сервера: 8.0.43
-- Версия PHP: 8.3.26

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `NearbyDB`
--

-- --------------------------------------------------------

--
-- Структура таблицы `balance_transactions`
--

CREATE TABLE `balance_transactions` (
  `id` int UNSIGNED NOT NULL,
  `user_id` int UNSIGNED NOT NULL,
  `amount` decimal(10,2) NOT NULL COMMENT 'Изменение баланса (может быть отрицательным)',
  `balance_after` decimal(10,2) NOT NULL COMMENT 'Баланс после операции',
  `type` enum('task_reward','task_cancellation','project_reward','withdrawal','refill') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `reference_type` enum('task','project') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `reference_id` int UNSIGNED DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='История изменений баланса';

--
-- Дамп данных таблицы `balance_transactions`
--

INSERT INTO `balance_transactions` (`id`, `user_id`, `amount`, `balance_after`, `type`, `reference_type`, `reference_id`, `created_at`) VALUES
(1, 6, 50.00, 50.00, 'task_reward', 'task', 3, '2026-03-30 10:05:30'),
(2, 6, 50.00, 100.00, 'task_reward', 'task', 3, '2026-03-30 10:23:56'),
(3, 6, 50.00, 150.00, 'task_reward', 'task', 3, '2026-03-30 10:25:07'),
(4, 6, 50.00, 200.00, 'task_reward', 'task', 3, '2026-04-01 11:10:58'),
(5, 6, 50.00, 250.00, 'task_reward', 'task', 3, '2026-04-01 11:14:38'),
(6, 6, 23.00, 273.00, 'task_reward', 'task', 4, '2026-04-08 21:12:51'),
(7, 6, 12.00, 285.00, 'task_reward', 'task', 6, '2026-05-03 00:44:29'),
(8, 7, 0.00, 0.00, 'task_reward', 'task', 7, '2026-05-07 17:38:36');

-- --------------------------------------------------------

--
-- Структура таблицы `chats`
--

CREATE TABLE `chats` (
  `id` int UNSIGNED NOT NULL,
  `type` enum('personal','group') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Название группового чата',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Чаты';

--
-- Дамп данных таблицы `chats`
--

INSERT INTO `chats` (`id`, `type`, `name`, `created_at`) VALUES
(1, 'personal', NULL, '2026-04-01 11:03:55'),
(2, 'personal', NULL, '2026-05-07 17:36:16'),
(3, 'group', 'Мега абобусы 2000', '2026-05-08 09:45:29'),
(4, 'personal', NULL, '2026-05-08 10:32:12');

-- --------------------------------------------------------

--
-- Структура таблицы `chat_members`
--

CREATE TABLE `chat_members` (
  `id` int UNSIGNED NOT NULL,
  `chat_id` int UNSIGNED NOT NULL,
  `user_id` int UNSIGNED NOT NULL,
  `joined_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `last_read_message_id` int UNSIGNED DEFAULT NULL COMMENT 'Последнее прочитанное сообщение'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Участники чатов';

--
-- Дамп данных таблицы `chat_members`
--

INSERT INTO `chat_members` (`id`, `chat_id`, `user_id`, `joined_at`, `last_read_message_id`) VALUES
(1, 1, 6, '2026-04-01 11:03:55', NULL),
(2, 1, 5, '2026-04-01 11:03:55', 1),
(3, 2, 6, '2026-05-07 17:36:16', NULL),
(4, 2, 7, '2026-05-07 17:36:16', NULL),
(5, 3, 6, '2026-05-08 09:45:29', NULL),
(6, 3, 7, '2026-05-08 09:45:29', NULL),
(7, 3, 5, '2026-05-08 09:45:29', NULL),
(8, 4, 5, '2026-05-08 10:32:12', NULL),
(9, 4, 7, '2026-05-08 10:32:12', NULL);

-- --------------------------------------------------------

--
-- Структура таблицы `messages`
--

CREATE TABLE `messages` (
  `id` int UNSIGNED NOT NULL,
  `chat_id` int UNSIGNED NOT NULL,
  `sender_id` int UNSIGNED NOT NULL COMMENT 'Отправитель (пользователь)',
  `content_type` enum('text','image','file','voice','notification') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'text',
  `content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT 'Текст сообщения или ссылка на файл',
  `file_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'URL файла (если есть)',
  `transcribed_text` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT 'Распознанный текст голосового сообщения',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `reply` int UNSIGNED DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Сообщения';

--
-- Дамп данных таблицы `messages`
--

INSERT INTO `messages` (`id`, `chat_id`, `sender_id`, `content_type`, `content`, `file_url`, `transcribed_text`, `created_at`, `reply`) VALUES
(1, 1, 5, 'text', 'string', 'string', 'string', '2026-04-01 11:41:49', NULL),
(2, 1, 5, 'text', '12345', NULL, NULL, '2026-04-08 06:09:25', NULL),
(3, 1, 5, 'text', '123', NULL, NULL, '2026-04-08 06:10:41', NULL),
(4, 1, 5, 'text', '123', NULL, NULL, '2026-04-08 06:11:11', NULL),
(5, 1, 5, 'text', '234', NULL, NULL, '2026-04-08 06:12:08', NULL),
(6, 1, 5, 'text', 'aboba\n2', NULL, NULL, '2026-04-08 07:01:23', NULL),
(7, 1, 5, 'text', 'привет\nпривет', NULL, NULL, '2026-04-08 21:09:29', NULL),
(8, 1, 5, 'text', 'Working?\nРаботает?', NULL, NULL, '2026-04-08 21:09:56', NULL),
(9, 1, 6, 'text', 'привет', NULL, NULL, '2026-04-08 21:12:12', NULL),
(11, 1, 5, 'text', '1234gvggh', NULL, NULL, '2026-05-06 23:19:53', NULL),
(12, 2, 7, 'text', '123567', NULL, NULL, '2026-05-07 17:36:22', NULL),
(14, 1, 5, 'text', '123123', NULL, NULL, '2026-05-08 08:47:44', NULL),
(15, 1, 5, 'text', 'Bye Bye', NULL, NULL, '2026-05-08 08:52:22', NULL),
(18, 3, 7, 'text', 'Привет, как у вас дела?', NULL, NULL, '2026-05-08 10:11:50', NULL),
(19, 3, 7, 'text', 'маааааааааааакккккккккссссссссииииииииммммммаааааалллллльььььнннннооооо\nдлинное сообщение', NULL, NULL, '2026-05-08 10:14:27', NULL),
(23, 4, 7, 'text', '500?\n123', NULL, NULL, '2026-05-08 10:35:43', NULL),
(24, 4, 7, 'text', '`12', NULL, NULL, '2026-05-08 10:38:50', NULL),
(25, 4, 7, 'text', '1\n2\n3\n4\n5\n6\n7\n8\n9\n10', NULL, NULL, '2026-05-08 10:52:28', NULL),
(26, 4, 7, 'text', '123\n123\n1\n23', NULL, NULL, '2026-05-08 10:52:36', NULL),
(27, 4, 7, 'text', '432', NULL, NULL, '2026-05-08 10:52:38', NULL),
(28, 4, 7, 'text', '6456', NULL, NULL, '2026-05-08 10:52:39', NULL),
(29, 4, 7, 'text', '1231245345345', NULL, NULL, '2026-05-08 10:56:25', NULL),
(30, 4, 7, 'text', 'dfgdgfdfg', NULL, NULL, '2026-05-08 10:56:32', NULL),
(31, 4, 7, 'text', 'fghfghfghjghj', NULL, NULL, '2026-05-08 10:56:35', NULL),
(32, 4, 7, 'text', 'sdfsfds', NULL, NULL, '2026-05-08 10:56:36', NULL),
(33, 4, 7, 'text', 'jghjgjh', NULL, NULL, '2026-05-08 10:56:38', NULL),
(34, 4, 5, 'text', '123', NULL, NULL, '2026-05-09 15:37:01', NULL),
(37, 1, 5, 'text', 'test', NULL, NULL, '2026-05-10 15:58:45', NULL),
(47, 3, 5, 'text', 'норм', NULL, NULL, '2026-05-11 14:03:05', 18),
(48, 3, 5, 'text', 'а теперь нет', NULL, NULL, '2026-05-11 14:04:54', 47),
(49, 3, 5, 'text', 'Привет!\nКак дела?\nЯ тут вспомнил тебя.\nДавно не виделись.\nМожет, встретимся?\nНапиши, когда сможешь.\nБуду ждать ответа.\nПока.', NULL, NULL, '2026-05-11 14:10:52', NULL),
(50, 3, 5, 'text', 'Слушай, а давай в пятницу?\nВообще хочется кофе и поболтать.\nУ тебя как с графиком?\nЯ свободна после шести.\nМожно в «Кофе и точке» у парка.\nИли где ты захочешь.\nОтветь, когда увидишь.\nБуду рада встрече.\nЦелую.', NULL, NULL, '2026-05-11 14:11:14', NULL),
(51, 3, 5, 'text', 'Привет! Как твои дела?\nЧто нового вообще?\nУ меня на работе аврал.\nДумаю о тебе часто.\nКак твоя кошка поживает?\nНапиши пару строчек.\nОчень не хватает общения.\nОбнимаю. До связи.', NULL, NULL, '2026-05-11 14:11:42', NULL),
(52, 3, 5, 'text', 'Доброе утро! Как спалось?\nСегодня отличная погода.\nМожет, погуляем вечером?\nЯ буду в центре в шесть.\nНапиши, где удобно встретиться.\nМожно просто пройтись по набережной.\nЖду твой ответ с надеждой.\nХорошего дня! Пока.', NULL, NULL, '2026-05-11 14:11:57', 48);

-- --------------------------------------------------------

--
-- Структура таблицы `notifications`
--

CREATE TABLE `notifications` (
  `id` int UNSIGNED NOT NULL,
  `user_id` int UNSIGNED NOT NULL,
  `type` enum('project_invite','task_invite','task_status_change','project_status_change','new_message','rating_received','organization_verified') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `reference_type` enum('task','project','chat','user') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `reference_id` int UNSIGNED DEFAULT NULL,
  `is_read` tinyint(1) DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Уведомления пользователей';

-- --------------------------------------------------------

--
-- Структура таблицы `organizations`
--

CREATE TABLE `organizations` (
  `id` int UNSIGNED NOT NULL,
  `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Название организации',
  `inn` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'ИНН (может отсутствовать)',
  `contact_person` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Контактное лицо (ФИО)',
  `contact_phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Контактный телефон',
  `contact_email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Контактный email',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT 'Описание деятельности',
  `logo` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Логотип',
  `verified` tinyint(1) DEFAULT '0' COMMENT 'Подтверждена модератором',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Юридические лица (организации)';

-- --------------------------------------------------------

--
-- Структура таблицы `organization_members`
--

CREATE TABLE `organization_members` (
  `id` int UNSIGNED NOT NULL,
  `user_id` int UNSIGNED NOT NULL COMMENT 'Пользователь',
  `organization_id` int UNSIGNED NOT NULL COMMENT 'Организация',
  `role` enum('owner','manager','member') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'member' COMMENT 'Роль внутри организации',
  `status` enum('pending','active') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT 'Статус членства (ожидает подтверждения / активен)',
  `added_by` int UNSIGNED DEFAULT NULL COMMENT 'Кто добавил/пригласил (ссылка на users)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Связь пользователей с организациями и роли в них';

-- --------------------------------------------------------

--
-- Структура таблицы `participant_days`
--

CREATE TABLE `participant_days` (
  `id` int UNSIGNED NOT NULL,
  `participant_id` int UNSIGNED NOT NULL,
  `date` date NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Даты участия (для типа A)';

-- --------------------------------------------------------

--
-- Структура таблицы `projects`
--

CREATE TABLE `projects` (
  `id` int UNSIGNED NOT NULL,
  `title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `type` enum('A','B') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'A - с ролями по дням, B - без разделения по дням',
  `created_by_user_id` int UNSIGNED DEFAULT NULL COMMENT 'Создатель – пользователь',
  `created_by_organization_id` int UNSIGNED DEFAULT NULL COMMENT 'Создатель – организация',
  `status` enum('searching','active','completed','cancelled') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'searching',
  `start_date` date DEFAULT NULL COMMENT 'Дата начала проекта',
  `end_date` date DEFAULT NULL COMMENT 'Дата окончания проекта',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Структура таблицы `project_participants`
--

CREATE TABLE `project_participants` (
  `id` int UNSIGNED NOT NULL,
  `user_id` int UNSIGNED NOT NULL,
  `project_id` int UNSIGNED NOT NULL,
  `role_id` int UNSIGNED NOT NULL,
  `status` enum('invited','active','rejected','left') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'invited',
  `invited_by` int UNSIGNED DEFAULT NULL COMMENT 'Кто пригласил (админ/овнер)',
  `joined_at` timestamp NULL DEFAULT NULL COMMENT 'Когда принял приглашение или был назначен'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Участники проектов и их роли';

-- --------------------------------------------------------

--
-- Структура таблицы `project_roles`
--

CREATE TABLE `project_roles` (
  `id` int UNSIGNED NOT NULL,
  `project_id` int UNSIGNED NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `total_needed` int UNSIGNED DEFAULT NULL COMMENT 'Для типа B: общее количество людей на эту роль'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Роли, доступные в проекте';

-- --------------------------------------------------------

--
-- Структура таблицы `project_role_daily_needs`
--

CREATE TABLE `project_role_daily_needs` (
  `id` int UNSIGNED NOT NULL,
  `role_id` int UNSIGNED NOT NULL,
  `date` date NOT NULL,
  `needed_count` int UNSIGNED NOT NULL COMMENT 'Сколько человек нужно на эту дату'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Ежедневная потребность в ролях (для типа A)';

-- --------------------------------------------------------

--
-- Структура таблицы `project_tasks`
--

CREATE TABLE `project_tasks` (
  `id` int UNSIGNED NOT NULL,
  `project_id` int UNSIGNED NOT NULL,
  `title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `date` date DEFAULT NULL COMMENT 'Если привязана к конкретному дню',
  `status` enum('pending','in_progress','completed') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Мелкие задачи внутри проекта';

-- --------------------------------------------------------

--
-- Структура таблицы `project_task_volunteers`
--

CREATE TABLE `project_task_volunteers` (
  `id` int UNSIGNED NOT NULL,
  `project_task_id` int UNSIGNED NOT NULL,
  `user_id` int UNSIGNED NOT NULL,
  `status` enum('assigned','completed') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'assigned',
  `assigned_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Волонтеры, выполняющие мелкие задачи проекта';

-- --------------------------------------------------------

--
-- Структура таблицы `ratings`
--

CREATE TABLE `ratings` (
  `id` int UNSIGNED NOT NULL,
  `rater_id` int UNSIGNED NOT NULL COMMENT 'Кто оценивает (пользователь)',
  `rated_user_id` int UNSIGNED NOT NULL COMMENT 'Кого оценивают (пользователь)',
  `rating` tinyint UNSIGNED NOT NULL,
  `comment` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `task_id` int UNSIGNED DEFAULT NULL COMMENT 'Ссылка на простую задачу (если оценка за неё)',
  `project_id` int UNSIGNED DEFAULT NULL COMMENT 'Ссылка на проект (если оценка за участие в проекте)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Триггеры `ratings`
--
DELIMITER $$
CREATE TRIGGER `trg_ratings_after_delete` AFTER DELETE ON `ratings` FOR EACH ROW BEGIN
    UPDATE users
    SET average_rating = (
        SELECT COALESCE(AVG(rating), 0)
        FROM ratings
        WHERE rated_user_id = OLD.rated_user_id
    )
    WHERE id = OLD.rated_user_id;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_ratings_after_insert` AFTER INSERT ON `ratings` FOR EACH ROW BEGIN
    UPDATE users
    SET average_rating = (
        SELECT COALESCE(AVG(rating), 0)
        FROM ratings
        WHERE rated_user_id = NEW.rated_user_id
    )
    WHERE id = NEW.rated_user_id;
END
$$
DELIMITER ;
DELIMITER $$
CREATE TRIGGER `trg_ratings_after_update` AFTER UPDATE ON `ratings` FOR EACH ROW BEGIN
    IF OLD.rated_user_id != NEW.rated_user_id THEN
        -- Для старого пользователя
        UPDATE users
        SET average_rating = (
            SELECT COALESCE(AVG(rating), 0)
            FROM ratings
            WHERE rated_user_id = OLD.rated_user_id
        )
        WHERE id = OLD.rated_user_id;

        -- Для нового пользователя
        UPDATE users
        SET average_rating = (
            SELECT COALESCE(AVG(rating), 0)
            FROM ratings
            WHERE rated_user_id = NEW.rated_user_id
        )
        WHERE id = NEW.rated_user_id;
    ELSE
        UPDATE users
        SET average_rating = (
            SELECT COALESCE(AVG(rating), 0)
            FROM ratings
            WHERE rated_user_id = NEW.rated_user_id
        )
        WHERE id = NEW.rated_user_id;
    END IF;
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Структура таблицы `tasks`
--

CREATE TABLE `tasks` (
  `id` int UNSIGNED NOT NULL,
  `title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `needed_volunteers` int UNSIGNED NOT NULL DEFAULT '1' COMMENT 'Сколько волонтеров нужно',
  `priority` enum('low','medium','high') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'medium',
  `location` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Место выполнения',
  `reward` decimal(10,2) DEFAULT '0.00' COMMENT 'Награда в нирбиках',
  `status` enum('searching','in_progress','completed','cancelled') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'searching',
  `created_by_user_id` int UNSIGNED DEFAULT NULL COMMENT 'Создатель – пользователь (если задача от физлица)',
  `created_by_organization_id` int UNSIGNED DEFAULT NULL COMMENT 'Создатель – организация (если задача от юрлица)',
  `deadline` datetime DEFAULT NULL COMMENT 'Срок выполнения',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Дамп данных таблицы `tasks`
--

INSERT INTO `tasks` (`id`, `title`, `description`, `needed_volunteers`, `priority`, `location`, `reward`, `status`, `created_by_user_id`, `created_by_organization_id`, `deadline`, `created_at`, `updated_at`) VALUES
(3, 'Попить со мной чай и поболтать', 'да да, благодарю благодарю', 1, 'low', 'хз', 50.00, 'completed', 5, NULL, '2026-03-24 00:00:00', '2026-03-23 10:20:17', '2026-04-01 11:14:38'),
(5, 'Купить продукты', 'Яблоки, мясо, печеньки, чай123', 2, 'medium', 'хз', 50.00, 'searching', 5, NULL, '2026-03-24 00:00:00', '2026-03-23 10:20:17', '2026-05-07 16:40:58');

-- --------------------------------------------------------

--
-- Структура таблицы `task_volunteers`
--

CREATE TABLE `task_volunteers` (
  `id` int UNSIGNED NOT NULL,
  `task_id` int UNSIGNED NOT NULL,
  `user_id` int UNSIGNED NOT NULL,
  `status` enum('pending','accepted','rejected','cancelled','completed') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT 'Статус участия',
  `assigned_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Волонтеры, выполняющие простые задачи';

--
-- Дамп данных таблицы `task_volunteers`
--

INSERT INTO `task_volunteers` (`id`, `task_id`, `user_id`, `status`, `assigned_at`, `updated_at`) VALUES
(2, 3, 6, 'completed', '2026-03-23 12:43:48', '2026-04-01 11:14:38'),
(4, 5, 6, 'rejected', '2026-04-08 22:09:31', '2026-05-04 20:07:26');

-- --------------------------------------------------------

--
-- Структура таблицы `users`
--

CREATE TABLE `users` (
  `id` int UNSIGNED NOT NULL,
  `full_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'ФИО',
  `city` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Город',
  `birth_date` date DEFAULT NULL COMMENT 'Год рождения',
  `about` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT 'Краткая информация о себе',
  `profile_picture` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Ссылка на фото профиля',
  `email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Email для входа',
  `phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Телефон',
  `password` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT 'Хеш пароля',
  `vk_id` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'ID ВКонтакте',
  `tg_id` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'ID Telegram',
  `balance` decimal(10,2) DEFAULT '0.00' COMMENT 'Текущий баланс нирбиков',
  `availability_status` enum('available','busy','offline') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'available' COMMENT 'Статус доступности',
  `is_admin` tinyint(1) DEFAULT '0',
  `is_moderator` tinyint(1) DEFAULT '0',
  `lastSeenAt` timestamp NULL DEFAULT NULL COMMENT 'Последнее посещение',
  `is_online` tinyint(1) DEFAULT '0' COMMENT 'Флаг онлайн',
  `average_rating` decimal(3,2) DEFAULT '0.00' COMMENT 'Средний рейтинг (на основе оценок)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `education_institution` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Учебное заведение',
  `education_degree` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Степень/квалификация',
  `education_field` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Специальность',
  `education_start_year` year DEFAULT NULL,
  `education_end_year` year DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Физические лица (пользователи)';

--
-- Дамп данных таблицы `users`
--

INSERT INTO `users` (`id`, `full_name`, `city`, `birth_date`, `about`, `profile_picture`, `email`, `phone`, `password`, `vk_id`, `tg_id`, `balance`, `availability_status`, `is_admin`, `is_moderator`, `lastSeenAt`, `is_online`, `average_rating`, `created_at`, `updated_at`, `education_institution`, `education_degree`, `education_field`, `education_start_year`, `education_end_year`) VALUES
(5, 'Вожегов Григорий Романович', 'Пермь2', '2006-06-07', 'Начинающий программист', NULL, 'tocabloha@gmail.com', '+79082697661', '$2y$10$y4VrRWP/21wRwpuxa5dl/.fcYssm5EsBRX2P5nieTJjFrCJpggZeW', NULL, NULL, 0.00, 'available', 0, 0, '2026-05-11 13:35:17', 1, 0.00, '2026-03-12 22:51:22', '2026-05-11 13:35:17', 'Пермский авиационный техникум имени А.Д. Швецова', 'Среднее специальное', 'Информационные системы и программирование', '2022', '2026'),
(6, 'test test2 test2', NULL, '2006-03-23', NULL, NULL, 'test@test.test', '89082697661', '$2y$10$Z.XSxoIjk0yqF86DJl0ZfO9PsGCrC8mfjgHSkTTgkn/WmZrYRY7sC', NULL, NULL, 285.00, 'available', 0, 0, '2026-05-04 19:57:10', 1, 0.00, '2026-03-18 11:22:05', '2026-05-04 19:57:10', NULL, NULL, NULL, '2022', '2026'),
(7, 'Иванов Иван Иванович', NULL, NULL, NULL, NULL, 'example@gmail.com', '+79082697668', '$2y$10$a/OQd5w/a9SbnXBiQZ1/mueYaR7y1olcn8qZQCFZ2.L3V3rTnYzvS', NULL, NULL, 0.00, 'available', 0, 0, '2026-05-08 10:11:26', 1, 0.00, '2026-05-07 17:18:52', '2026-05-08 10:11:25', NULL, NULL, NULL, NULL, NULL);

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `balance_transactions`
--
ALTER TABLE `balance_transactions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_created` (`created_at`);

--
-- Индексы таблицы `chats`
--
ALTER TABLE `chats`
  ADD PRIMARY KEY (`id`);

--
-- Индексы таблицы `chat_members`
--
ALTER TABLE `chat_members`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_chat_user` (`chat_id`,`user_id`),
  ADD KEY `user_id` (`user_id`),
  ADD KEY `fk_last_read_message` (`last_read_message_id`);

--
-- Индексы таблицы `messages`
--
ALTER TABLE `messages`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_chat` (`chat_id`),
  ADD KEY `idx_sender` (`sender_id`),
  ADD KEY `messages_ibfk_3` (`reply`);

--
-- Индексы таблицы `notifications`
--
ALTER TABLE `notifications`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_user` (`user_id`),
  ADD KEY `idx_read` (`is_read`);

--
-- Индексы таблицы `organizations`
--
ALTER TABLE `organizations`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_name` (`name`);

--
-- Индексы таблицы `organization_members`
--
ALTER TABLE `organization_members`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_member` (`user_id`,`organization_id`),
  ADD KEY `organization_id` (`organization_id`),
  ADD KEY `added_by` (`added_by`);

--
-- Индексы таблицы `participant_days`
--
ALTER TABLE `participant_days`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_participant_day` (`participant_id`,`date`);

--
-- Индексы таблицы `projects`
--
ALTER TABLE `projects`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_status` (`status`),
  ADD KEY `idx_creator_user` (`created_by_user_id`),
  ADD KEY `idx_creator_org` (`created_by_organization_id`);

--
-- Индексы таблицы `project_participants`
--
ALTER TABLE `project_participants`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_project_user_role` (`user_id`,`project_id`,`role_id`),
  ADD KEY `project_id` (`project_id`),
  ADD KEY `role_id` (`role_id`),
  ADD KEY `invited_by` (`invited_by`);

--
-- Индексы таблицы `project_roles`
--
ALTER TABLE `project_roles`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_project` (`project_id`);

--
-- Индексы таблицы `project_role_daily_needs`
--
ALTER TABLE `project_role_daily_needs`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_role_date` (`role_id`,`date`);

--
-- Индексы таблицы `project_tasks`
--
ALTER TABLE `project_tasks`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_project` (`project_id`);

--
-- Индексы таблицы `project_task_volunteers`
--
ALTER TABLE `project_task_volunteers`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_task_volunteer` (`project_task_id`,`user_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Индексы таблицы `ratings`
--
ALTER TABLE `ratings`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_task_rating` (`rater_id`,`rated_user_id`,`task_id`),
  ADD UNIQUE KEY `unique_project_rating` (`rater_id`,`rated_user_id`,`project_id`),
  ADD KEY `task_id` (`task_id`),
  ADD KEY `project_id` (`project_id`),
  ADD KEY `idx_rater` (`rater_id`),
  ADD KEY `idx_rated` (`rated_user_id`);

--
-- Индексы таблицы `tasks`
--
ALTER TABLE `tasks`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_status` (`status`),
  ADD KEY `idx_creator_user` (`created_by_user_id`),
  ADD KEY `idx_creator_org` (`created_by_organization_id`);

--
-- Индексы таблицы `task_volunteers`
--
ALTER TABLE `task_volunteers`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_task_volunteer` (`task_id`,`user_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Индексы таблицы `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `idx_email` (`email`),
  ADD KEY `idx_vk` (`vk_id`),
  ADD KEY `idx_tg` (`tg_id`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `balance_transactions`
--
ALTER TABLE `balance_transactions`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT для таблицы `chats`
--
ALTER TABLE `chats`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT для таблицы `chat_members`
--
ALTER TABLE `chat_members`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT для таблицы `messages`
--
ALTER TABLE `messages`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=53;

--
-- AUTO_INCREMENT для таблицы `notifications`
--
ALTER TABLE `notifications`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `organizations`
--
ALTER TABLE `organizations`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `organization_members`
--
ALTER TABLE `organization_members`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `participant_days`
--
ALTER TABLE `participant_days`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `projects`
--
ALTER TABLE `projects`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `project_participants`
--
ALTER TABLE `project_participants`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `project_roles`
--
ALTER TABLE `project_roles`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `project_role_daily_needs`
--
ALTER TABLE `project_role_daily_needs`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `project_tasks`
--
ALTER TABLE `project_tasks`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `project_task_volunteers`
--
ALTER TABLE `project_task_volunteers`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `ratings`
--
ALTER TABLE `ratings`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT для таблицы `tasks`
--
ALTER TABLE `tasks`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT для таблицы `task_volunteers`
--
ALTER TABLE `task_volunteers`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT для таблицы `users`
--
ALTER TABLE `users`
  MODIFY `id` int UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- Ограничения внешнего ключа сохраненных таблиц
--

--
-- Ограничения внешнего ключа таблицы `balance_transactions`
--
ALTER TABLE `balance_transactions`
  ADD CONSTRAINT `balance_transactions_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `chat_members`
--
ALTER TABLE `chat_members`
  ADD CONSTRAINT `chat_members_ibfk_1` FOREIGN KEY (`chat_id`) REFERENCES `chats` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `chat_members_ibfk_2` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_last_read_message` FOREIGN KEY (`last_read_message_id`) REFERENCES `messages` (`id`) ON DELETE SET NULL;

--
-- Ограничения внешнего ключа таблицы `messages`
--
ALTER TABLE `messages`
  ADD CONSTRAINT `messages_ibfk_1` FOREIGN KEY (`chat_id`) REFERENCES `chats` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `messages_ibfk_2` FOREIGN KEY (`sender_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `messages_ibfk_3` FOREIGN KEY (`reply`) REFERENCES `messages` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `notifications`
--
ALTER TABLE `notifications`
  ADD CONSTRAINT `notifications_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `organization_members`
--
ALTER TABLE `organization_members`
  ADD CONSTRAINT `organization_members_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `organization_members_ibfk_2` FOREIGN KEY (`organization_id`) REFERENCES `organizations` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `organization_members_ibfk_3` FOREIGN KEY (`added_by`) REFERENCES `users` (`id`) ON DELETE SET NULL;

--
-- Ограничения внешнего ключа таблицы `participant_days`
--
ALTER TABLE `participant_days`
  ADD CONSTRAINT `participant_days_ibfk_1` FOREIGN KEY (`participant_id`) REFERENCES `project_participants` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `projects`
--
ALTER TABLE `projects`
  ADD CONSTRAINT `projects_ibfk_1` FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `projects_ibfk_2` FOREIGN KEY (`created_by_organization_id`) REFERENCES `organizations` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `project_participants`
--
ALTER TABLE `project_participants`
  ADD CONSTRAINT `project_participants_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `project_participants_ibfk_2` FOREIGN KEY (`project_id`) REFERENCES `projects` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `project_participants_ibfk_3` FOREIGN KEY (`role_id`) REFERENCES `project_roles` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `project_participants_ibfk_4` FOREIGN KEY (`invited_by`) REFERENCES `users` (`id`) ON DELETE SET NULL;

--
-- Ограничения внешнего ключа таблицы `project_roles`
--
ALTER TABLE `project_roles`
  ADD CONSTRAINT `project_roles_ibfk_1` FOREIGN KEY (`project_id`) REFERENCES `projects` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `project_role_daily_needs`
--
ALTER TABLE `project_role_daily_needs`
  ADD CONSTRAINT `project_role_daily_needs_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `project_roles` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `project_tasks`
--
ALTER TABLE `project_tasks`
  ADD CONSTRAINT `project_tasks_ibfk_1` FOREIGN KEY (`project_id`) REFERENCES `projects` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `project_task_volunteers`
--
ALTER TABLE `project_task_volunteers`
  ADD CONSTRAINT `project_task_volunteers_ibfk_1` FOREIGN KEY (`project_task_id`) REFERENCES `project_tasks` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `project_task_volunteers_ibfk_2` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `ratings`
--
ALTER TABLE `ratings`
  ADD CONSTRAINT `ratings_ibfk_1` FOREIGN KEY (`rater_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `ratings_ibfk_2` FOREIGN KEY (`rated_user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `ratings_ibfk_3` FOREIGN KEY (`task_id`) REFERENCES `tasks` (`id`) ON DELETE SET NULL,
  ADD CONSTRAINT `ratings_ibfk_4` FOREIGN KEY (`project_id`) REFERENCES `projects` (`id`) ON DELETE SET NULL;

--
-- Ограничения внешнего ключа таблицы `tasks`
--
ALTER TABLE `tasks`
  ADD CONSTRAINT `tasks_ibfk_1` FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `tasks_ibfk_2` FOREIGN KEY (`created_by_organization_id`) REFERENCES `organizations` (`id`) ON DELETE CASCADE;

--
-- Ограничения внешнего ключа таблицы `task_volunteers`
--
ALTER TABLE `task_volunteers`
  ADD CONSTRAINT `task_volunteers_ibfk_1` FOREIGN KEY (`task_id`) REFERENCES `tasks` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `task_volunteers_ibfk_2` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
