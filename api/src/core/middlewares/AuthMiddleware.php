<?php

namespace Middleware;

use Models\UsersQuery;
use Core\JWToken;
use Core\Request;
use Core\Response;
use Core\Routes;
use Exception;

class AuthMiddleware implements MiddlewareInterface
{
    private Routes $routes;
    public function __construct(Routes $routes)
    {
        $this->routes = $routes;
    }
    public function handle(Request $request, callable $next): Response
    {
        $route = $this->routes->exists($request);
        if($route === null){
            return new Response(404, ['error' => 'Wrong api route or method']);
        }
        $request->route = $route;
        if($route['auth'] === false){
            return $next($request);
        }
        if (session_status() === PHP_SESSION_NONE) {
            session_start();
        }
        if (empty($_SESSION)) {
            $jwt = $request->cookie['jwt'] ?? null;
            if($jwt) {
                $_SESSION = JWToken::validateToken($jwt) ?? [];
            }
        }
        if (!$this->isAuthenticated($_SESSION)) {
            return new Response(401, ['error'=>'Unauthorized']);
        }
        return $next($request);
    }
    
    public static function isAuthenticated($payload): bool
    {
        try{
            $p = $payload['phone'] ?? null;
            $e = $payload['email'] ?? null;
            if($p && $e){
                $v = UsersQuery::create()
                ->filterByPhone($payload['phone'])
                ->findOneByEmail($payload['email']);
                if($v){
                    return true;
                }
            }
            return false;
        }catch(Exception $e){
            return false;
        }
    }
}
?>