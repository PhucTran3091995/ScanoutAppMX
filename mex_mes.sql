CREATE DATABASE  IF NOT EXISTS `mex_mes` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `mex_mes`;
-- MySQL dump 10.13  Distrib 8.0.42, for Win64 (x86_64)
--
-- Host: 10.7.10.6    Database: mex_mes
-- ------------------------------------------------------
-- Server version	8.0.42

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `tb_aoi_ng`
--

DROP TABLE IF EXISTS `tb_aoi_ng`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tb_aoi_ng` (
  `ID` int NOT NULL,
  `ARRAY_ID` varchar(45) NOT NULL,
  `PID` varchar(45) NOT NULL,
  `ARRAY_INDEX` varchar(45) NOT NULL,
  `COMPONENT` varchar(45) NOT NULL,
  `INSP_TYPE` varchar(45) NOT NULL,
  `USER_RESULT` varchar(45) NOT NULL,
  `END_TIME` datetime NOT NULL,
  `END_DATE` date NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tb_aoi_ng`
--

LOCK TABLES `tb_aoi_ng` WRITE;
/*!40000 ALTER TABLE `tb_aoi_ng` DISABLE KEYS */;
/*!40000 ALTER TABLE `tb_aoi_ng` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tb_aoi_pid`
--

DROP TABLE IF EXISTS `tb_aoi_pid`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tb_aoi_pid` (
  `id` int NOT NULL AUTO_INCREMENT,
  `LINE` varchar(10) NOT NULL,
  `WORK_FACE` varchar(3) NOT NULL,
  `WORK_ORDER` varchar(45) NOT NULL,
  `ARRAY_ID` varchar(45) NOT NULL,
  `PID` varchar(45) NOT NULL,
  `PROGRAM_NAME` varchar(255) NOT NULL,
  `MODEL` varchar(45) NOT NULL,
  `MACHINE_RESULT` varchar(4) NOT NULL,
  `USER_RESULT` varchar(3) NOT NULL,
  `END_DATE` date NOT NULL,
  `END_TIME` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tb_aoi_pid`
--

LOCK TABLES `tb_aoi_pid` WRITE;
/*!40000 ALTER TABLE `tb_aoi_pid` DISABLE KEYS */;
/*!40000 ALTER TABLE `tb_aoi_pid` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'mex_mes'
--

--
-- Dumping routines for database 'mex_mes'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-09-23 15:35:44
