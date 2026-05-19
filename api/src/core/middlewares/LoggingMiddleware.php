<?php

namespace Middleware;

use Core\Request;
use Core\Response;

class LoggingMiddleware implements MiddlewareInterface
{
    public function handle(Request $request, callable $next): Response
    {
        $response = $next($request);
        error_log(message: "Request: {$request->method} {$request->uri} {$request->body}\nResponse: {$response->status}");
        return $response;
    }
}
?>