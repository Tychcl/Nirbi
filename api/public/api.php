<?php
use Core\MiddlewareFabric;
use Core\Router;
use Core\Request;
use Core\Routes;

require_once dirname(__DIR__) . '/vendor/autoload.php';
require_once dirname(__DIR__) . '/propel/generated/config.php';

$routes = new Routes();
$router = new Router();

$routes->add('Api\UserController');
$routes->add('Api\TasksController');
$routes->add('Api\ChatsController');
$dispatcher = MiddlewareFabric::createForApi($router, $routes);

$request = new Request();
$response = $dispatcher->handle($request);
$response->send();
?>