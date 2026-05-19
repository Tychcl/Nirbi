<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Вход/Регистрация</title>
    <style>
        body { font-family: Arial; max-width: 400px; margin: 50px auto; background: lightgray;}
        .all {background: white; border-radius: 7px; padding: 10px;}
        .tabs { display: flex; margin-bottom: 20px; }
        .tab { flex: 1; padding: 10px; border: none; background: #212121; cursor: pointer; }
        .tab.active { background: #007bff; color: white; }
        .form { display: none;}
        .form.active { display: block; }
        input { width: 100%; padding: 10px; margin: 5px 0; box-sizing: border-box; }
        button { width: 100%; padding: 12px; background: #007bff; color: white; border: none; cursor: pointer; }
        .error { color: red; font-size: 14px; }
        .success { color: green; }
    </style>
    <script>
        const REGEX = {
            phone: /^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$/,
            email: /^([a-z0-9_-]+\.)*[a-z0-9_-]+@[a-z0-9_-]+(\.[a-z0-9_-]+)*\.[a-z]{2,6}$/i
        };

        function showForm(formName) {
            document.querySelectorAll('.form').forEach(f => f.classList.remove('active'));
            document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
            document.getElementById(formName + 'Form').classList.add('active');
            document.querySelector(`.tab[onclick="showForm('${formName}')"]`).classList.add('active');
            document.getElementById(formName + 'Error').textContent = '';
        }

        async function register(event) {
            event.preventDefault(); //отмена действия по умолчанию
            const form = event.target;
            const errorEl = document.getElementById('registerError');
            errorEl.textContent = '';
            errorEl.className = 'error';

            const data = {
                name: form.name.value.trim(),
                phone: form.phone.value.trim(),
                email: form.email.value.trim().toLowerCase(),
                password: form.password.value,
                confirm: form.confirm.value
            };

            if (!data.name || !data.phone || !data.email || !data.password || !data.confirm) {
                return showError('registerError', 'Заполните все поля');
            }
            
            if (data.password !== data.confirm) {
                return showError('registerError', 'Пароли не совпадают');
            }
            
            if (!REGEX.phone.test(data.phone.replace(/\s/g, ''))) { //убераем все не числа
                return showError('registerError', 'Неверный формат телефона');
            }
            
            if (!REGEX.email.test(data.email)) {
                return showError('registerError', 'Неверный формат email');
            }

            try {
                const response = await fetch('/api/users/regin', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                const result = await response.json();
                
                if (response.ok) {
                    errorEl.textContent = 'Регистрация успешна!';
                    errorEl.className = 'success';
                    form.reset();
                } else {
                    showError('registerError', result.error || 'Ошибка регистрации');
                }
            } catch (error) {
                showError('registerError', 'Ошибка сети');
            }
        }

        async function loginFormSubmit(event) {
            event.preventDefault();
            const form = event.target;
            const errorEl = document.getElementById('loginError');
            errorEl.textContent = '';
            errorEl.className = 'error';

            const loginInput = form.login.value.trim();
            const password = form.password.value;
            const captcha = form.querySelector('input[name="smart-token"]').value

            if (!loginInput || !password) {
                return showError('loginError', 'Заполните все поля');
            }

            if (!captcha) {
                return showError('loginError', 'Решите капчу');
            }

            const isPhone = REGEX.phone.test(loginInput.replace(/\s/g, ''));
            const isEmail = REGEX.email.test(loginInput);
            
            if (!isPhone && !isEmail) {
                return showError('loginError', 'Неверный формат телефона или почты');
            }

            const data = {
                login: loginInput,
                password: password,
                captcha: captcha
            };

            try {
                const response = await fetch('/api/users/signin', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'same-origin',
                    body: JSON.stringify(data)
                });

                const result = await response.json();
                
                if (response.ok) {
                    errorEl.textContent = 'Вход выполнен!';
                    errorEl.className = 'success';
                    form.reset();
                    window.location.href = '/profile';
                } else {
                    showError('loginError', result.error || 'Ошибка входа');
                }
            } catch (error) {
                console.log(error)
                showError('loginError', 'Ошибка сети');
            }
        }

        function showError(id, message) {
            const el = document.getElementById(id);
            el.textContent = message;
            el.className = 'error';
        }
    </script>
    <script src="https://smartcaptcha.cloud.yandex.ru/captcha.js" defer></script>
</head>
<body>
    <div class="all">
    <div class="tabs">
        <button class="tab active" onclick="showForm('register')">Регистрация</button>
        <button class="tab" onclick="showForm('login')">Вход</button>
    </div>

    <form id="registerForm" class="form active" onsubmit="register(event)">
        <input type="text" name="name" placeholder="Имя" required>
        <input type="text" name="phone" placeholder="Телефон" required>
        <input type="email" name="email" placeholder="Email" required>
        <input type="password" name="password" placeholder="Пароль" required>
        <input type="password" name="confirm" placeholder="Подтвердите пароль" required>
        <button type="submit">Зарегистрироваться</button>
        <div class="error" id="registerError"></div>
    </form>

    <form id="loginForm" class="form" onsubmit="loginFormSubmit(event)">
        <input type="text" name="login" placeholder="Телефон или Email" required>
        <input type="password" name="password" placeholder="Пароль" required>
        <div id="captcha-container"
            class="smart-captcha"
            data-sitekey="<?= $key ?>"
            allow="accelerometer"></div>
        <button type="submit">Войти</button>
        <div class="error" id="loginError"></div>
    </form>

    </div>
</body>
</html>