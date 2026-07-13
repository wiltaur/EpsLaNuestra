USE [master]
GO

/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [sa]    Script Date: 7/12/2026 6:55:43 PM ******/
CREATE LOGIN [developer] WITH PASSWORD=N'Mytestdb.123', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=ON, CHECK_POLICY=ON
GO

ALTER LOGIN [developer] ENABLE
GO

ALTER SERVER ROLE [sysadmin] ADD MEMBER [developer]
GO