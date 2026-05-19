<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Мой профиль</title>
    <style>
        body { font-family: Arial; max-width: 400px; margin: 50px auto; background: lightgray;}
        .all {background: white; border-radius: 7px; padding: 10px;}
        .form { margin-bottom: 20px; }
        h2 { text-align: center; margin-bottom: 20px; }
        .info { 
            background: #f5f5f5; 
            padding: 10px; 
            margin: 10px 0; 
            border-radius: 5px; 
            font-size: 14px;
        }
        .info label { 
            display: inline-block; 
            width: 120px; 
            color: #666; 
        }
        .info span { font-weight: bold; }
        input { 
            width: 100%; 
            padding: 10px; 
            margin: 5px 0; 
            box-sizing: border-box; 
        }
        button { 
            width: 100%; 
            padding: 12px; 
            background: #007bff; 
            color: white; 
            border: none; 
            cursor: pointer; 
            margin: 5px 0;
        }
        .btn-logout { background: #dc3545; }
        .message { 
            padding: 10px; 
            border-radius: 5px; 
            margin: 10px 0; 
            display: none;
        }
        .success { 
            background: #d4edda; 
            color: #155724; 
            display: block;
        }
        .error { 
            background: #f8d7da; 
            color: #721c24; 
            display: block;
        }
    </style>
    <script>
        const userData = {
            name: '<?= htmlspecialchars($userName); ?>',
            email: '<?= htmlspecialchars($userEmail); ?>',
            phone: '<?= htmlspecialchars($userPhone); ?>'
        };

        async function updateProfile(event) {
            event.preventDefault();
            const form = event.target;
            const messageEl = document.getElementById('profile-message');
            
            const data = {
                name: form.name.value.trim(),
                email: form.email.value.trim(),
                phone: form.phone.value.trim(),
                currentPassword: form.currentPassword.value
            };

            // Валидация
            if (!data.currentPassword) {
                showMessage('profile-message', 'Введите текущий пароль', 'error');
                return;
            }

            if (!data.name || !data.email || !data.phone) {
                showMessage('profile-message', 'Заполните все поля', 'error');
                return;
            }

            try {
                const response = await fetch('/api/users', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                const result = await response.json();
                
                if (response.ok) {
                    showMessage('profile-message', 'Данные успешно обновлены!', 'success');
                    form.reset();
                    document.getElementById('display-name').textContent = data.name;
                    document.getElementById('display-email').textContent = data.email;
                    document.getElementById('display-phone').textContent = data.phone;
                } else {
                    showMessage('profile-message', result.error || 'Ошибка', 'error');
                }
            } catch (error) {
                console.log(error)
                showMessage('profile-message', 'Ошибка сети', 'error');
            }
        }

        async function changePassword(event) {
            event.preventDefault();
            const form = event.target;
            const messageEl = document.getElementById('password-message');
            
            const data = {
                old: form.old.value,
                new: form.new.value,
                confirm: form.confirm.value
            };

            if (!data.old || !data.new || !data.confirm) {
                showMessage('password-message', 'Заполните все поля', 'error');
                return;
            }

            if (data.new !== data.confirm) {
                showMessage('password-message', 'Пароли не совпадают', 'error');
                return;
            }

            try {
                const response = await fetch('/api/users/password', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                const result = await response.json();
                
                if (response.ok) {
                    showMessage('password-message', 'Пароль успешно изменен!', 'success');
                    form.reset();
                } else {
                    showMessage('password-message', result.error || 'Ошибка', 'error');
                }
            } catch (error) {
                showMessage('password-message', 'Ошибка сети', 'error');
            }
        }

        async function logout() {
            try {
                const response = await fetch('/api/users/logout', {
                    method: 'POST',
                    credentials: 'same-origin',
                    headers: { 'Content-Type': 'application/json' }
                });

                if (response.ok) {
                    window.location.href = '/';
                }
            } catch (error) {
                alert('Ошибка при выходе');
            }
        }

        function showMessage(id, text, type) {
            const el = document.getElementById(id);
            el.textContent = text;
            el.className = 'message ' + type;
        }

        document.addEventListener('DOMContentLoaded', function() {
            document.getElementById('name').value = userData.name;
            document.getElementById('email').value = userData.email;
            document.getElementById('phone').value = userData.phone;
        });
    </script>
</head>
<body>
    <div class="all">
        <h2>Мой профиль</h2>
        
        <div class="info">
            <label>Имя:</label> <span id="display-name"><?= htmlspecialchars($userName); ?></span>
        </div>
        <div class="info">
            <label>Email:</label> <span id="display-email"><?= htmlspecialchars($userEmail); ?></span>
        </div>
        <div class="info">
            <label>Телефон:</label> <span id="display-phone"><?= htmlspecialchars($userPhone); ?></span>
        </div>

        <div class="message" id="profile-message"></div>
        <div class="message" id="password-message"></div>

        <form class="form" onsubmit="updateProfile(event)">
            <input type="text" id="name" name="name" placeholder="Имя" required>
            <input type="email" id="email" name="email" placeholder="Email" required>
            <input type="tel" id="phone" name="phone" placeholder="Телефон" required>
            <input type="password" name="currentPassword" placeholder="Текущий пароль (для подтверждения)" required>
            <button type="submit">Обновить данные</button>
        </form>

        <form class="form" onsubmit="changePassword(event)">
            <input type="password" name="old" placeholder="Текущий пароль" required>
            <input type="password" name="new" placeholder="Новый пароль" required>
            <input type="password" name="confirm" placeholder="Повторите новый пароль" required>
            <button type="submit">Изменить пароль</button>
        </form>

        <button class="btn-logout" onclick="logout()">Выйти из аккаунта</button>
    </div>
</body>
</html>