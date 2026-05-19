<?php
namespace Web;

use Classes\Render;
use Core\Response;
use Core\Route;
use Middleware\AuthMiddleware;
use Models\UsersQuery;
use Dotenv\Dotenv;

require_once dirname(__DIR__,3) . '/vendor/autoload.php';

#[Route("")]
class PagesController{

    #[Route("", 'GET', false)]
    public function mainPage(){
        if (session_status() === PHP_SESSION_NONE) {
            session_start();
        }
        
        if (AuthMiddleware::isAuthenticated($_SESSION)) {
            error_log("Authenticated");
            $response = new Response(302);
            $response->headers[] = Response::LOC.'/profile';
            return $response;
        }
        $dotenv = Dotenv::createImmutable(dirname(__DIR__,3));
        $dotenv->load(); 
        return new Response(200, ['echo' => Render::renderTemplate(fileName: '/auth.php', data: ['key' => $_ENV['CAPTCHAKEY']])]);
    }

    #[Route("/profile", 'GET', true)]
    public function profile(){
        if (session_status() === PHP_SESSION_NONE) {
            session_start();
        }
        error_log('profile');
        if (!AuthMiddleware::isAuthenticated($_SESSION)) {
            $response = new Response(302);
            $response->headers[] = 'Location: /';
            return $response;
        }
        $user = UsersQuery::create()->findOneById($_SESSION['id']);
        if (!$user) {
            $response = new Response(302);
            $response->headers[] = 'Location: /';
            return $response;
        }
        return new Response(200, ['echo' => Render::renderTemplate(fileName: '/profile.php', data: ['userName' => $user->getName(),'userEmail' => $user->getEmail(), 'userPhone' => $user->getPhone()])]);
    }

}