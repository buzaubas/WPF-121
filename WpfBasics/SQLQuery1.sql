SELECT
	MAX( D.Name )  AS  N'Отдел',
	COUNT( M.Id )  AS  N'Сотрудников'
FROM
	Managers M
	JOIN Departments D ON M.Id_main_dep = D.Id
GROUP BY
	D.Id