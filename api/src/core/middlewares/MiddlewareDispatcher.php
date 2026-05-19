<?php

namespace Middleware;

use Core\Request;
use Core\Response;

class MiddlewareDispatcher
{
    private $middlewares = [];
    
    public function add(MiddlewareInterface $middleware): void
    {
        $this->middlewares[] = $middleware;
    }
    
    public function handle(Request $request): Response
    {
        $handler = $this->createHandler();
        return $handler($request);
    }
    
    private function createHandler(): callable
    {
        $handler = function (Request $request) {
            return new Response(404, ['error'=>'Page not found']);
        };
        
        foreach (array_reverse($this->middlewares) as $middleware) {
            $handler = function (Request $request) use ($middleware, $handler) {
                return $middleware->handle($request, $handler);
            };
        }
        
        return $handler;
    }
}

?>