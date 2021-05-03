<?php
require("core/php/imports.php");
header('Content-Type: application/json');

$data = $data = array(
    'nope' => "HAHH"
);
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    if($_POST["islogin"] == true){
        http_response_code(404);
        if(isset($_POST["password"]) == "123") {
            http_response_code(200);
            $data = array(
                'username' => $_POST["username"],
                'pass' => $_POST["password"]
            );
        }
    }
}
echo json_encode($data);
?>