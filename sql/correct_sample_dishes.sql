-- Исправляет демонстрационные записи, если ранее был запущен старый seed_sample_data.sql.
-- Сопоставление сделано по реальным именам и содержимому фотографий из ImagesTemp.zip.

BEGIN;

INSERT INTO public.category_bluda (category_id, category_name) VALUES
    (5, 'Закуски'),
    (6, 'Завтраки')
ON CONFLICT (category_id) DO NOTHING;

INSERT INTO public.products (product_id, product_name, belky, jiry, uglevody)
VALUES (21, 'Яблоко', 0, 0, 14)
ON CONFLICT (product_id) DO UPDATE
SET product_name = EXCLUDED.product_name,
    belky = EXCLUDED.belky,
    jiry = EXCLUDED.jiry,
    uglevody = EXCLUDED.uglevody;

UPDATE public.bluda SET bludo_name = 'Бастурма', id_categirya = 5, osnova = 'говядина', vihod = 120, bludo_image = 'basturma.jpg' WHERE bludo_id = 1;
UPDATE public.bluda SET bludo_name = 'Омлет с луком', id_categirya = 6, osnova = 'яйца', vihod = 180, bludo_image = 'omelet_s_lukom.jpg' WHERE bludo_id = 2;
UPDATE public.bluda SET bludo_name = 'Рулет с яблоками', id_categirya = 4, osnova = 'тесто', vihod = 180, bludo_image = 'rulet_s_yablokami.jpg' WHERE bludo_id = 3;
UPDATE public.bluda SET bludo_name = 'Салат летний', id_categirya = 2, osnova = 'овощи', vihod = 180, bludo_image = 'salat_letniy.jpg' WHERE bludo_id = 4;
UPDATE public.bluda SET bludo_name = 'Творожная масса', id_categirya = 4, osnova = 'творог', vihod = 180, bludo_image = 'tvorog.jpg' WHERE bludo_id = 5;
UPDATE public.bluda SET bludo_name = 'Харчо', id_categirya = 1, osnova = 'говядина', vihod = 300, bludo_image = 'kharcho.jpg' WHERE bludo_id = 6;

UPDATE public.rezepty SET rezept = 'Говядину засолить, натереть специями и выдержать до готовности. Нарезать тонкими ломтиками.' WHERE bludo_id = 1;
UPDATE public.rezepty SET rezept = 'Взбить яйца с молоком и солью. Добавить лук и приготовить омлет на сковороде.' WHERE bludo_id = 2;
UPDATE public.rezepty SET rezept = 'Испечь бисквитный корж, добавить начинку из яблок, свернуть рулетом и остудить.' WHERE bludo_id = 3;
UPDATE public.rezepty SET rezept = 'Нарезать свежие овощи и зелень, посолить и заправить растительным маслом.' WHERE bludo_id = 4;
UPDATE public.rezepty SET rezept = 'Смешать творог с небольшим количеством молока и сахара до однородной массы.' WHERE bludo_id = 5;
UPDATE public.rezepty SET rezept = 'Сварить говядину. Добавить рис, помидоры, чеснок и зелень; варить до готовности.' WHERE bludo_id = 6;

DELETE FROM public.sostav_blud WHERE bludo_id BETWEEN 1 AND 6;
INSERT INTO public.sostav_blud (bludo_id, product_id, ves_bluda) VALUES
    (1, 16, 120), (1, 20, 3),
    (2, 9, 120), (2, 3, 25), (2, 12, 25), (2, 13, 5),
    (3, 21, 50), (3, 10, 50), (3, 9, 25), (3, 11, 10),
    (4, 6, 90), (4, 7, 70), (4, 19, 5), (4, 13, 10),
    (5, 17, 150), (5, 12, 20), (5, 11, 10),
    (6, 16, 70), (6, 5, 25), (6, 6, 30), (6, 20, 3), (6, 19, 5);

SELECT setval('public.category_bluda_category_id_seq', (SELECT max(category_id) FROM public.category_bluda));
SELECT setval('public.products_product_id_seq', (SELECT max(product_id) FROM public.products));

COMMIT;
