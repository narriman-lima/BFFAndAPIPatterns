@echo off
REM Script para rodar todos os microservices em paralelo

REM Definir onde está a solução
cd /d "%~dp0"

echo.
echo ========================================
echo Starting Microservices Architecture
echo ========================================
echo.
echo Launching services in separate windows...
echo.

REM Iniciar cada serviço em uma nova janela
start "UserService" cmd /k "cd src\Services\UserService && echo Starting UserService at http://localhost:5003 && dotnet run"
timeout /t 1 /nobreak

start "OrderService" cmd /k "cd src\Services\OrderService && echo Starting OrderService at http://localhost:5004 && dotnet run"
timeout /t 1 /nobreak

start "ProductService" cmd /k "cd src\Services\ProductService && echo Starting ProductService at http://localhost:5005 && dotnet run"
timeout /t 1 /nobreak

start "Aggregator" cmd /k "cd src\Aggregator && echo Starting Aggregator at http://localhost:5002 && dotnet run"
timeout /t 1 /nobreak

start "Bff" cmd /k "cd src\Bff && echo Starting Bff at http://localhost:5001 && dotnet run"
timeout /t 1 /nobreak

start "ApiGateway" cmd /k "cd src\ApiGateway && echo Starting ApiGateway at http://localhost:5000 && dotnet run"

echo.
echo ========================================
echo All services are starting in separate windows!
echo ========================================
echo.
echo Test the application:
echo   - API Gateway:  http://localhost:5000/dashboard
echo   - Bff:      http://localhost:5001/swagger
echo   - Aggregator:   http://localhost:5002/swagger
echo   - UserService:  http://localhost:5003/swagger
echo   - OrderService: http://localhost:5004/swagger
echo   - ProductServ:  http://localhost:5005/swagger
echo.
pause

