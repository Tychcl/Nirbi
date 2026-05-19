<?php
namespace Classes;

use Core\Response;
use Exception;
use Models\UsersQuery;

class Validate
{
    public static function Ex(Exception $ex)
    {
        return new Response(500, $ex->getMessage());
    }

    /**
     * Проверка наличия всех обязательных параметров
     * @param array $required Список обязательных полей
     * @param array $data Входные данные
     * @return bool true, если хотя бы одного поля нет или оно пустое
     */
    public static function checkParams($required, $data): bool
    {
        foreach ($required as $field) {
            $value = $data[$field] ?? null;
            if ($value === null || trim($value) === '') {
                return true;
            }
        }
        return false;
    }

    /**
     * Валидация телефона
     * @param string $phone
     * @param Response|null $response Ссылка на объект ответа для заполнения в случае ошибки
     * @return bool true, если формат неверный
     */
    public static function phone($phone, &$response = null): bool
    {
        if (preg_match("/^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$/", $phone)) {
            return false;
        }
        if ($response !== null) {
            $response = new Response(400, ['error' => 'Wrong phone format']);
        }
        return true;
    }

    /**
     * Валидация email
     * @param string $email
     * @param Response|null $response
     * @return bool
     */
    public static function email($email, &$response = null): bool
    {
        if (preg_match("/^([a-z0-9_-]+\.)*[a-z0-9_-]+@[a-z0-9_-]+(\.[a-z0-9_-]+)*\.[a-z]{2,6}$/i", $email)) {
            return false;
        }
        if ($response !== null) {
            $response = new Response(400, ['error' => 'Wrong email format']);
        }
        return true;
    }

    /**
     * Валидация даты в формате YYYY-MM-DD
     * @param string $date
     * @param Response|null $response
     * @return bool true, если дата корректна
     */
    public static function date($date, &$response = null): bool
    {
        $d = \DateTime::createFromFormat('Y-m-d', $date);
        if ($d && $d->format('Y-m-d') === $date) {
            return true;
        }
        if ($response !== null) {
            $response = new Response(400, ['error' => 'Wrong date format, expected YYYY-MM-DD']);
        }
        return false;
    }

    /**
     * Проверка уникальности значения по указанному полю
     * @param string $field Название поля в БД (phone, email и т.д.)
     * @param mixed $value Значение для проверки
     * @param Response|null $response
     * @param int|null $excludeUserId ID пользователя, который исключается из проверки (при редактировании)
     * @return bool true, если пользователь с таким значением уже существует
     */
    public static function findUserByField($field, $value, &$response = null, $excludeUserId = null): bool
    {
        $query = UsersQuery::create()->filterBy($field, $value);
        if ($excludeUserId) {
            $query->filterById($excludeUserId, \Propel\Runtime\ActiveQuery\Criteria::NOT_EQUAL);
        }
        $user = $query->findOne();
        if ($user) {
            if ($response !== null) {
                $response = new Response(409, ['error' => 'User with this ' . $field . ' already exists']);
            }
            return true;
        }
        return false;
    }
}