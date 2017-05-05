use LoggingWarehouse

GRANT EXEC ON LoggingWarehouse.logging.spA_CheckLogService TO []
GRANT EXEC ON LoggingWarehouse.logging.spA_CheckLogServiceInstance TO []
GRANT EXEC ON LoggingWarehouse.logging.spa_SaveAdditionalDatas TO []
GRANT EXEC ON LoggingWarehouse.logging.spa_saveLogMsg TO []
GRANT EXEC ON LoggingWarehouse.logging.spa_SaveLogs TO []
GRANT EXEC ON LoggingWarehouse.logging.spa_saveMessages TO []
GRANT EXEC ON LoggingWarehouse.logging.spa_saveStackTraces TO []
GRANT EXEC ON LoggingWarehouse.logging.spa_SavingCallingMethodParamsIOs TO []


GRANT EXECUTE ON TYPE::logging.AdditionalDatas TO []
GRANT EXECUTE ON TYPE::logging.AppLogs TO []
GRANT EXECUTE ON TYPE::logging.CallingMethodIOs TO []
GRANT EXECUTE ON TYPE::logging.LogMessages TO []
GRANT EXECUTE ON TYPE::logging.logServices TO []
GRANT EXECUTE ON TYPE::logging.logServicesInstances TO []
GRANT EXECUTE ON TYPE::logging.messages TO []
GRANT EXECUTE ON TYPE::logging.stackTraces TO [] 