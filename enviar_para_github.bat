@echo off
title Enviar Analisador Lexico para o GitHub
cd /d "c:\Users\ccvca\Desktop\Compiladores"
echo ========================================================
echo   Enviando o projeto para o GitHub...
echo   Repositorio: https://github.com/GuiDSousa/Analisador-Lexico.git
echo ========================================================
echo.
git push -u origin main
echo.
if %ERRORLEVEL% EQU 0 (
    echo [SUCESSO] O projeto foi enviado com exito para o GitHub!
) else (
    echo [AVISO] Se solicitou login, faca a autenticacao na janela do navegador.
)
echo.
pause
