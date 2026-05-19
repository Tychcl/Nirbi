<?php
use Core\MiddlewareFabric;
use Core\Router;
use Core\Request;
use Core\Routes;

require_once dirname(__DIR__) . '/vendor/autoload.php';
require_once dirname(__DIR__) . '/propel/generated/config.php';

$routes = new Routes();
$router = new Router();

$routes->add('Web\PagesController');
$dispatcher = MiddlewareFabric::createForWeb($router, $routes);

$request = new Request();
$response = $dispatcher->handle($request);

$response->send();
?>