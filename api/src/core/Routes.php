<?php
namespace Core;

use ReflectionClass;
use ReflectionMethod;

class Routes{

    public $routes = [];

    public function add($controller){
        $Reflect = new ReflectionClass($controller);
        $Route = $Reflect->getAttributes(Route::class)[0]->newInstance();
        $ConPath = $Route->path;

        $methods = get_class_methods($controller);
        foreach($methods as $m){
            $Reflect = new ReflectionMethod($controller,$m);
            $Route = $Reflect->getAttributes(Route::class)[0]->newInstance();

            $this->routes[] = [
                'method' => strtoupper($Route->method),
                'path' => strtolower($ConPath.$Route->path),
                'handler' => $controller.'@'.$m,
                'auth' => $Route->requiredAuth
            ];
        }
    }

    public function exists(Request $request){
        if (strpos($request->uri, 'favicon.ico') !== false) {
            return null;
        }
        $uri = strtolower(rtrim($request->uri, '/'));
        $path = parse_url($uri, PHP_URL_PATH);

        if ($path === '/favicon.ico' || $path === 'favicon.ico') {
            return null;
        }
        #error_log(json_encode($request));
        foreach ($this->routes as $route) {
            if ($route['method'] !== $request->method) {
                continue;
            }
            $routePath = rtrim($route['path'], '/');
            error_log(json_encode(['path' => $path, 'route' => $routePath]));
            if ($routePath === $path) {
                return $route;
            }
            $pattern = preg_replace('/\{[a-z]+\}/', '([^/]+)', $routePath);
            $pattern = '#^' . $pattern . '$#';
            if (preg_match($pattern, $path, $matches)) {
                return $route;
            }
        }
        return null;
    }
}
?>