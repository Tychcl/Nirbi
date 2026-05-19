<?php
namespace Core;

use Exception;
use Firebase\JWT\JWT;
use Firebase\JWT\Key;
//use Dotenv\Dotenv;

class JWToken
{
    public static function generateToken($payload)
    {
        //$dotenv = Dotenv::createImmutable(dirname(__DIR__,2));
        //$dotenv->load();

        $secretKey = $_ENV['JWT'];
        $expire = time() + (60 * 60 * 6);
        
        $tokenPayload = array(
            "iat" => time(), // время выпуска
            "exp" => $expire, // время истечения
            "data" => $payload // данные пользователя
        );
        
        return JWT::encode($tokenPayload, $secretKey, 'HS256');
    }
    
    public static function validateToken($token)
    {
        try {
            //$dotenv = Dotenv::createImmutable(dirname(__DIR__,2));
            //$dotenv->load();
            $secretKey = $_ENV['JWT'];
            $decoded = JWT::decode($token, new Key($secretKey, 'HS256'));
            return (array) $decoded->data;
        } catch(Exception $e) {
            return false;
        }
    }
}
?>