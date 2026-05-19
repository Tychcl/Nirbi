<?php

namespace Core;

use Middleware\MiddlewareDispatcher;
use Middleware\LoggingMiddleware;
use Middleware\AuthMiddleware;
use Middleware\RouterMiddleware;
use Core\Routes;

class MiddlewareFabric
{

    public static function Create_Common(){
        $dispatcher = new MiddlewareDispatcher();
        $dispatcher->add(new LoggingMiddleware());
        return $dispatcher;
    }

    public static function createForWeb(Router $router, Routes $routes): MiddlewareDispatcher
    {
        $dispatcher = MiddlewareFabric::Create_Common();
        $dispatcher->add(new AuthMiddleware($routes));
        $dispatcher->add(new RouterMiddleware($router));
        return $dispatcher;
    }
    
    public static function createForApi(Router $router, Routes $routes): MiddlewareDispatcher
    {
        $dispatcher = MiddlewareFabric::Create_Common();
        $dispatcher->add(new AuthMiddleware($routes));
        $dispatcher->add(new RouterMiddleware($router));
        return $dispatcher;
    }
}