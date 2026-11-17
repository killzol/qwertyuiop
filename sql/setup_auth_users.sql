-- Роли и демонстрационные учетные записи из файла «Доп_сущности.xlsx».
-- Запустить один раз в базе «Кулинария»; повторный запуск обновит этих пользователей.

BEGIN;

INSERT INTO public.roles (role_id, role_name) VALUES
    (1, 'Администратор'),
    (2, 'Менеджер'),
    (3, 'Шеф-повар')
ON CONFLICT (role_id) DO UPDATE
SET role_name = EXCLUDED.role_name;

ALTER TABLE public.users
    ALTER COLUMN role_id SET DEFAULT 3;

DROP INDEX IF EXISTS public.ux_users_login_lower;

INSERT INTO public.users
    (user_id, first_name, last_name, patronymic, date_of_birthday, login, password, phone, adress, role_id)
VALUES
    (1, 'Василий', 'Петров', 'Петрович', DATE '1997-01-12', 'vp', '500eda2ef2e24cbe303842619261ff675966cc01eae266e949cc81731578cb4faa2cc05985333b8b9d02563b118558efdfe990ad3565aa553dae665dad7e7bea', '9217894562', 'СПб', 2),
    (2, 'Геннадий', 'Крокодилов', 'Иванович', DATE '1999-02-15', 'GK', 'fa8d3dcafc067b5f46b3f175315a104ebb4c3e2ab14d90474cc41bf2ec94382789a3d82dc7be3addb094b7a2749bc37a67d7d8e9f927582af6b6f42a65e9281c', '9112365489', 'СПб', 1),
    (3, 'Андрей', 'Иванов', 'Сидорович', DATE '2001-03-20', 'AI', 'e09cc2272237ec8c6e7bfdafc39b2b4e88d7e33e44a4c7ffacfea6ade0c961258896c08f118c1b891170affc77406f19fc5ccc7e326a80aa54187f97fa4cc3a8', '9111234565', 'СПб', 2),
    -- В Excel хеш для Kp12345@ был на один символ короче. Здесь записан верный SHA-512.
    (4, 'Клавдий', 'Плюшкин', 'Андреевич', DATE '2000-05-25', 'KP', 'feb13f2bbea8580b6c840b1868b987411852cb9b203afcd787a807bf4f06a7a518c91f30a3579c99b2af384e1d1a627b2267cb558357a0da17523b1370c8eb26', '9817854261', 'СПб', 3)
ON CONFLICT (user_id) DO UPDATE SET
    first_name = EXCLUDED.first_name,
    last_name = EXCLUDED.last_name,
    patronymic = EXCLUDED.patronymic,
    date_of_birthday = EXCLUDED.date_of_birthday,
    login = EXCLUDED.login,
    password = EXCLUDED.password,
    phone = EXCLUDED.phone,
    adress = EXCLUDED.adress,
    role_id = EXCLUDED.role_id;

SELECT setval('public.roles_role_id_seq', GREATEST((SELECT max(role_id) FROM public.roles), 3));
SELECT setval('public.users_user_id_seq', GREATEST((SELECT max(user_id) FROM public.users), 4));

COMMIT;
