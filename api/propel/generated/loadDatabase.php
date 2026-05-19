<?php
$serviceContainer = \Propel\Runtime\Propel::getServiceContainer();
$serviceContainer->initDatabaseMapFromDumps(array (
  'default' => 
  array (
    'tablesByName' => 
    array (
      'balance_transactions' => '\\Models\\Map\\BalanceTransactionsTableMap',
      'chat_members' => '\\Models\\Map\\ChatMembersTableMap',
      'chats' => '\\Models\\Map\\ChatsTableMap',
      'messages' => '\\Models\\Map\\MessagesTableMap',
      'notifications' => '\\Models\\Map\\NotificationsTableMap',
      'organization_members' => '\\Models\\Map\\OrganizationMembersTableMap',
      'organizations' => '\\Models\\Map\\OrganizationsTableMap',
      'participant_days' => '\\Models\\Map\\ParticipantDaysTableMap',
      'project_participants' => '\\Models\\Map\\ProjectParticipantsTableMap',
      'project_role_daily_needs' => '\\Models\\Map\\ProjectRoleDailyNeedsTableMap',
      'project_roles' => '\\Models\\Map\\ProjectRolesTableMap',
      'project_task_volunteers' => '\\Models\\Map\\ProjectTaskVolunteersTableMap',
      'project_tasks' => '\\Models\\Map\\ProjectTasksTableMap',
      'projects' => '\\Models\\Map\\ProjectsTableMap',
      'ratings' => '\\Models\\Map\\RatingsTableMap',
      'task_volunteers' => '\\Models\\Map\\TaskVolunteersTableMap',
      'tasks' => '\\Models\\Map\\TasksTableMap',
      'users' => '\\Models\\Map\\UsersTableMap',
    ),
    'tablesByPhpName' => 
    array (
      '\\BalanceTransactions' => '\\Models\\Map\\BalanceTransactionsTableMap',
      '\\ChatMembers' => '\\Models\\Map\\ChatMembersTableMap',
      '\\Chats' => '\\Models\\Map\\ChatsTableMap',
      '\\Messages' => '\\Models\\Map\\MessagesTableMap',
      '\\Notifications' => '\\Models\\Map\\NotificationsTableMap',
      '\\OrganizationMembers' => '\\Models\\Map\\OrganizationMembersTableMap',
      '\\Organizations' => '\\Models\\Map\\OrganizationsTableMap',
      '\\ParticipantDays' => '\\Models\\Map\\ParticipantDaysTableMap',
      '\\ProjectParticipants' => '\\Models\\Map\\ProjectParticipantsTableMap',
      '\\ProjectRoleDailyNeeds' => '\\Models\\Map\\ProjectRoleDailyNeedsTableMap',
      '\\ProjectRoles' => '\\Models\\Map\\ProjectRolesTableMap',
      '\\ProjectTaskVolunteers' => '\\Models\\Map\\ProjectTaskVolunteersTableMap',
      '\\ProjectTasks' => '\\Models\\Map\\ProjectTasksTableMap',
      '\\Projects' => '\\Models\\Map\\ProjectsTableMap',
      '\\Ratings' => '\\Models\\Map\\RatingsTableMap',
      '\\TaskVolunteers' => '\\Models\\Map\\TaskVolunteersTableMap',
      '\\Tasks' => '\\Models\\Map\\TasksTableMap',
      '\\Users' => '\\Models\\Map\\UsersTableMap',
    ),
  ),
));
