<?php

namespace Classes;

class Render{

    public static function create_options($array){
        if(empty($array)){
            return '';
        }
        $keys = array_keys($array[0]);
        $str = '';
        foreach($array as $e){
            $str = $str.'<option value="'.$e[$keys[0]].'">'.$e[$keys[1]].'</option>';
        }
        return $str;
    }

    public static function renderTemplate($templatedir = "", $data = [], $fileName = 'template.php') {
        extract($data);
        ob_start();
        if(!$templatedir){
            $fileName = trim($fileName, '/');
        }
        include dirname(__DIR__).'/pages/'.trim($templatedir, '/').$fileName;
        return ob_get_clean();
    }

}

?>