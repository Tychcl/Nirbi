<?php

namespace Core;

class Response {
    public $status;
    public $body;
    public $headers = [];
    public $cookies = [];

    public const HTML = 'Content-Type: text/html; charset=utf-8';
    public const JSON = 'Content-Type: application/json; charset=utf-8';
    public const LOC = 'Location: ';
    
    public function __construct($status = 200, $body = null) {
        $this->setSecurityHeaders();
        $this->status = $status;
        $this->body = $body;
    }
    
    private function setSecurityHeaders() {
        $this->headers[] = 'X-Content-Type-Options: nosniff';
        $this->headers[] = 'X-Frame-Options: DENY';
        $this->headers[] = 'X-XSS-Protection: 1; mode=block';
        
        $this->headers[] = 'Permissions-Policy: accelerometer=*';

        $csp = [
            "default-src 'self'",
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://*.yandex.ru",
            "style-src 'self' 'unsafe-inline'",
            "img-src 'self' data: https://*.yandex.ru",
            "connect-src 'self' https://*.yandex.ru",
            "frame-src 'self' https://*.yandex.ru",
            "frame-ancestors 'none'"
        ];
        $this->headers[] = 'Content-Security-Policy: ' . implode('; ', $csp);
    }

    public function setCook($name, $value, $expire = 0, $path = '', $domain = '', $secure = true, $httponly = true, $samesite = 'Strict') {
        $this->cookies[] = [
            'name' => $name,
            'value' => $value,
            'expire' => $expire,
            'path' => $path,
            'domain' => $domain,
            'secure' => $secure,
            'httponly' => $httponly,
            'samesite' => $samesite
        ];
    }
    
    private function preSend(){
        http_response_code($this->status);
        
        foreach ($this->headers as $header) {
            header($header);
        }

        foreach ($this->cookies as $cookie) {
            setcookie(
                $cookie['name'],
                $cookie['value'],
                [
                    'expires' => $cookie['expire'],
                    'path' => $cookie['path'],
                    'domain' => $cookie['domain'],
                    'secure' => $cookie['secure'],
                    'httponly' => $cookie['httponly'],
                    'samesite' => $cookie['samesite']
                ]
            );
        }
    }

    public function send() {
        $this->preSend();
        if ($this->body !== null) {

            if ($this->status >= 300 && $this->status < 400) {
                return;
            }

            if (is_array($this->body) && !isset($this->body['echo'])) {
                header(Response::JSON);
                echo json_encode($this->body, JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT | JSON_UNESCAPED_UNICODE);
            } else if (isset($this->body['echo'])) {
                header(Response::HTML);
                echo $this->body['echo'];
            } else {
                header(Response::HTML);
                echo htmlspecialchars($this->body, ENT_QUOTES, 'UTF-8');
        }
    }
    }
}