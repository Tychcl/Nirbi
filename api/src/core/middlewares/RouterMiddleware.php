<?php

namespace Middleware;

use Core\Request;
use Core\Response;
use Core\Router;
use Override;

class RouterMiddleware implements MiddlewareInterface
{
    private Router $router;

    public function __construct(Router $router)
    {
        $this->router = $router;
    }

    public function handle(Request $request, callable $next): Response
    {
        return $this->router->dispatch($request);
    }
}