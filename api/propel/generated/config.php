<?php
use Dotenv\Dotenv;

require_once dirname(__DIR__,2) . '/vendor/autoload.php';

$dotenv = Dotenv::createImmutable(dirname(__DIR__,2));
$dotenv->load(); 

$host = $_ENV['DATABASE_HOST'];
$port = $_ENV['DATABASE_PORT'];
$dbname = $_ENV['DATABASE_NAME'];
$user = $_ENV['DATABASE_USER'];
$pwd = $_ENV['DATABASE_PASSWORD'];
$dsn = "mysql:host={$host};port={$port};dbname={$dbname}";

$serviceContainer = \Propel\Runtime\Propel::getServiceContainer();
$serviceContainer->checkVersion(2);
$serviceContainer->setAdapterClass('default', 'mysql');
$manager = new \Propel\Runtime\Connection\ConnectionManagerSingle('default');
$manager->setConfiguration(array (
  'dsn' => $dsn,
  'user' => $user,
  'password' => $pwd,
  'settings' =>
  array (
    'charset' => 'utf8',
    'queries' =>
    array (
    ),
  ),
  'classname' => '\\Propel\\Runtime\\Connection\\ConnectionWrapper',
  'model_paths' =>
  array (
    0 => 'src',
    1 => 'vendor',
  ),
));
$serviceContainer->setConnectionManager($manager);
$serviceContainer->setDefaultDatasource('default');
require_once __DIR__ . '/loadDatabase.php';