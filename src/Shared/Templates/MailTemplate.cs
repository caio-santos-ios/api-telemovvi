namespace api_infor_cell.src.Shared.Templates
{

    public static class MailTemplate
    {
        private static readonly string UiURI =  Environment.GetEnvironmentVariable("UI_URI") ?? "";
        public static string ForgotPasswordWeb(string name, string code)
        {
            return $@"
                <html>
                    <head>
                        <style>
                            .container {{
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                background-color: #ffffff;
                                padding: 40px;
                                border-radius: 10px;
                                max-width: 600px;
                                margin: 20px auto;
                                color: #333;
                                border: 1px solid #e0e0e0;
                                box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                            }}
                            .header {{
                                text-align: center;
                                color: #2c3e50;
                                border-bottom: 2px solid #34495e;
                                padding-bottom: 20px;
                                margin-bottom: 20px;
                            }}
                            .instruction {{
                                font-size: 16px;
                                line-height: 1.6;
                                text-align: center;
                            }}
                            .code-display {{
                                background-color: #f4f7f6;
                                border: 1px solid #cfd8dc;
                                padding: 15px;
                                text-align: center;
                                font-size: 28px;
                                font-weight: bold;
                                color: #2c3e50;
                                margin: 25px 0;
                                border-radius: 5px;
                                letter-spacing: 4px;
                            }}
                            .alert {{
                                background-color: #fff3cd;
                                color: #856404;
                                padding: 15px;
                                border-radius: 5px;
                                font-size: 13px;
                                margin-top: 25px;
                                border-left: 5px solid #ffeeba;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 12px;
                                color: #95a5a6;
                                text-align: center;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header"">
                                <h2>Recuperação de Senha</h2>
                            </div>
                            
                            <p class=""instruction"">Olá <strong>{name}</strong>,</p>
                            <p class=""instruction"">Recebemos uma solicitação para redefinir a senha da sua conta na <strong>Telemovvi</strong>. Se foi você, utilize o link abaixo:</p>
                            
                            <div class=""flex justify-center items-center"">
                                <a class='text-center' href='{UiURI}/reset-password/{code}'>Link para alterar senha</a>
                            </div>
                                                        
                            <div class=""alert"">
                                <strong>Segurança:</strong> Se você não solicitou a alteração da sua senha, ignore este e-mail. Sua senha atual permanecerá segura e nenhuma ação será tomada.
                            </div>
                            
                            <div class=""footer"">
                                <hr style=""border: 0; border-top: 1px solid #eee;"" />
                                <p>Atenciosamente,<br><strong>Equipe Telemovvi</strong></p>
                                <p>Este é um e-mail automático, por favor não responda.</p>
                            </div>
                        </div>
                    </body>
                </html>";
        }
        public static string ForgotPasswordApp(string code)
        {
            return $@"
                <html>
                    <head>
                        <style>
                        .container {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            padding: 20px;
                            border-radius: 8px;
                            max-width: 600px;
                            margin: auto;
                            color: #333;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 10px 20px;
                            margin-top: 20px;
                            background-color: #007bff;
                            color: #fff;
                            text-decoration: none;
                            border-radius: 5px;
                        }}
                        .footer {{
                            margin-top: 30px;
                            font-size: 12px;
                            color: #888;
                        }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                        <h2>Redefinição de Senha</h2>
                        <p>Você solicitou a alteração da sua senha.</p>
                        <p>Código de alteração da senha: {code}.</p>                        
                        <p>Se você não solicitou esta alteração, ignore este e-mail.</p>
                        <div class=""footer"">
                            <p>Este é um e-mail automático. Não responda esta mensagem.</p>
                        </div>
                        </div>
                    </body>
                </html>";
        }
        public static string FirstAccess(string name, string email, string passowrd)
        {
            return $@"               
                <html>
                    <head>
                        <style>
                            .container {{
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                background-color: #ffffff;
                                padding: 40px;
                                border-radius: 10px;
                                max-width: 600px;
                                margin: 20px auto;
                                color: #333;
                                border: 1px solid #e0e0e0;
                                box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                            }}
                            .header {{
                                text-align: center;
                                border-bottom: 2px solid #007bff;
                                padding-bottom: 20px;
                                margin-bottom: 20px;
                            }}
                            .code-box {{
                                background-color: #f8f9fa;
                                border: 2px dashed #007bff;
                                padding: 20px;
                                text-align: center;
                                font-size: 32px;
                                font-weight: bold;
                                letter-spacing: 5px;
                                color: #007bff;
                                margin: 30px 0;
                                border-radius: 8px;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 13px;
                                color: #777;
                                text-align: center;
                                line-height: 1.6;
                            }}
                            .welcome-text {{
                                font-size: 18px;
                                margin-bottom: 10px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header"">
                                <h1>Bem-vindo à Telemovvi!</h1>
                            </div>
                            
                            <p class=""welcome-text"">Olá, <strong>{name}</strong>,</p>
                            <p>Dados do primeiro acesso ao sistema:</p>

                            <p>E-mail: {email}</p>                        
                            <p>Senha: {passowrd}</p>      
                            <a href=""{UiURI}"">Fazer Login</a>                            
                            <p>Este código expira em 5 minutos. Se você não solicitou a criação desta conta, por favor, ignore este e-mail.</p>
                            
                            <div class=""footer"">
                                <hr style=""border: 0; border-top: 1px solid #eee;"" />
                                <p>Atenciosamente,<br><strong>Equipe Telemovvi</strong></p>
                                <p>Este é um e-mail automático, por favor não responda.</p>
                            </div>
                        </div>
                    </body>
                </html>
            ";
        }
        public static string ConfirmAccount(string name, string code)
        {
            return $@"
                <html>
                    <head>
                        <style>
                            .container {{
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                background-color: #ffffff;
                                padding: 40px;
                                border-radius: 10px;
                                max-width: 600px;
                                margin: 20px auto;
                                color: #333;
                                border: 1px solid #e0e0e0;
                                box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                            }}
                            .header {{
                                text-align: center;
                                border-bottom: 2px solid #007bff;
                                padding-bottom: 20px;
                                margin-bottom: 20px;
                            }}
                            .code-box {{
                                background-color: #f8f9fa;
                                border: 2px dashed #007bff;
                                padding: 20px;
                                text-align: center;
                                font-size: 32px;
                                font-weight: bold;
                                letter-spacing: 5px;
                                color: #007bff;
                                margin: 30px 0;
                                border-radius: 8px;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 13px;
                                color: #777;
                                text-align: center;
                                line-height: 1.6;
                            }}
                            .welcome-text {{
                                font-size: 18px;
                                margin-bottom: 10px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header"">
                                <h1>Bem-vindo à Telemovvi!</h1>
                            </div>
                            
                            <p class=""welcome-text"">Olá, <strong>{name}</strong>,</p>
                            <p>Ficamos felizes em ter você conosco. Para concluir a criação da sua conta e garantir a segurança dos seus dados, utilize o código de verificação abaixo:</p>
                            
                            <div class=""code-box"">
                                {code}
                            </div>
                            
                            <p>Este código expira em 5 minutos. Se você não solicitou a criação desta conta, por favor, ignore este e-mail.</p>
                            
                            <div class=""footer"">
                                <hr style=""border: 0; border-top: 1px solid #eee;"" />
                                <p>Atenciosamente,<br><strong>Equipe Telemovvi</strong></p>
                                <p>Este é um e-mail automático, por favor não responda.</p>
                            </div>
                        </div>
                    </body>
                </html>";
        }
        public static string NewCodeConfirmAccount(string name, string code)
        {
            return $@"
                <html>
                    <head>
                        <style>
                            .container {{
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                background-color: #ffffff;
                                padding: 40px;
                                border-radius: 10px;
                                max-width: 600px;
                                margin: 20px auto;
                                color: #333;
                                border: 1px solid #e0e0e0;
                                box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                            }}
                            .header {{
                                text-align: center;
                                color: #007bff; /* Cor de alerta/atenção */
                                border-bottom: 2px solid #007bff;
                                padding-bottom: 20px;
                                margin-bottom: 20px;
                            }}
                            .code-box {{
                                background-color: #f8f9fa;
                                border: 2px dashed #007bff;
                                padding: 20px;
                                text-align: center;
                                font-size: 32px;
                                font-weight: bold;
                                letter-spacing: 5px;
                                color: #007bff;
                                margin: 30px 0;
                                border-radius: 8px;
                            }}
                            .info-text {{
                                font-size: 16px;
                                line-height: 1.5;
                                text-align: center;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 12px;
                                color: #999;
                                text-align: center;
                            }}
                            .warning {{
                                background-color: #fff3cd;
                                color: #856404;
                                padding: 10px;
                                border-radius: 5px;
                                font-size: 14px;
                                margin-top: 20px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header"">
                                <h2>Novo Código de Verificação</h2>
                            </div>
                            
                            <p class=""info-text"">Olá <strong>{name}</strong>,</p>
                            <p class=""info-text"">Você solicitou um novo código de acesso para a sua conta na <strong>Telemovvi</strong>. Utilize o código abaixo para prosseguir:</p>
                            
                            <div class=""code-box"">
                                {code}
                            </div>
                            
                            <div class=""warning"">
                                <strong>Atenção:</strong> Este código é válido por 5 minutos. Por segurança, não compartilhe este código com ninguém.
                            </div>
                            
                            <p class=""info-text"" style=""margin-top: 20px;"">Se você não solicitou este código, ignore este e-mail ou entre em contato com nosso suporte.</p>
                            
                            <div class=""footer"">
                                <hr style=""border: 0; border-top: 1px solid #eee;"" />
                                <p>Atenciosamente,<br><strong>Equipe Telemovvi</strong></p>
                                <p>Este é um e-mail automático, por favor não responda.</p>
                            </div>
                        </div>
                    </body>
                </html>";
        }
        public static string NewLinkCodeConfirmAccount(string name, string code)
        {
            return $@"
                <html>
                    <head>
                        <style>
                            .container {{
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                background-color: #ffffff;
                                padding: 40px;
                                border-radius: 10px;
                                max-width: 600px;
                                margin: 20px auto;
                                color: #333;
                                border: 1px solid #e0e0e0;
                                box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                            }}
                            .header {{
                                text-align: center;
                                color: #007bff; /* Cor de alerta/atenção */
                                border-bottom: 2px solid #007bff;
                                padding-bottom: 20px;
                                margin-bottom: 20px;
                            }}
                            .code-box {{
                                background-color: #f8f9fa;
                                border: 2px dashed #007bff;
                                padding: 20px;
                                text-align: center;
                                font-size: 32px;
                                font-weight: bold;
                                letter-spacing: 5px;
                                color: #007bff;
                                margin: 30px 0;
                                border-radius: 8px;
                            }}
                            .info-text {{
                                font-size: 16px;
                                line-height: 1.5;
                                text-align: center;
                            }}
                            .footer {{
                                margin-top: 30px;
                                font-size: 12px;
                                color: #999;
                                text-align: center;
                            }}
                            .warning {{
                                background-color: #fff3cd;
                                color: #856404;
                                padding: 10px;
                                border-radius: 5px;
                                font-size: 14px;
                                margin-top: 20px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header"">
                                <h2>Novo Link de Confirmação de Conta</h2>
                            </div>
                            
                            <p class=""info-text"">Olá <strong>{name}</strong>,</p>
                            <p class=""info-text"">Você solicitou um novo link de confirmação de conta <strong>Telemovvi</strong>. Utilize o link abaixo para prosseguir:</p>
                            
                            <a href=""{UiURI}/confirm-account/{code}"">Confirmar conta</a>
                            
                            <div class=""warning"">
                                <strong>Atenção:</strong> Este link é válido por 5 minutos. Por segurança, não compartilhe este link com ninguém.
                            </div>
                            
                            <p class=""info-text"" style=""margin-top: 20px;"">Se você não solicitou este código, ignore este e-mail ou entre em contato com nosso suporte.</p>
                            
                            <div class=""footer"">
                                <hr style=""border: 0; border-top: 1px solid #eee;"" />
                                <p>Atenciosamente,<br><strong>Equipe Telemovvi</strong></p>
                                <p>Este é um e-mail automático, por favor não responda.</p>
                            </div>
                        </div>
                    </body>
                </html>";
        }

        public static string NewEmployee(
            string name,
            string nameEmpresa,
            string cargo,
            string email,
            string senhaProvisoira
        )
        {
            return $@"
                <html>
                <head>
                <title>Bem-vindo à equipe! — Telemovvi</title>
                <style>
                    @import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');
                    * {{ margin:0; padding:0; box-sizing:border-box; }}
                    body {{ background:#0F0A1A; font-family:'Outfit',sans-serif; -webkit-font-smoothing:antialiased; }}
                    .wrapper {{ max-width:620px; margin:0 auto; padding:40px 16px; }}
                    .card {{ background:#1A1028; border-radius:20px; overflow:hidden; border:1px solid rgba(113,39,167,0.3); }}
                    .header {{ background:linear-gradient(135deg,#2D0F4E 0%,#7127A7 60%,#A862DC 100%); padding:48px 40px 56px; text-align:center; position:relative; overflow:hidden; }}
                    .header::before {{ content:''; position:absolute; top:-60px; right:-60px; width:220px; height:220px; border-radius:50%; background:rgba(255,255,255,0.05); }}
                    .header::after  {{ content:''; position:absolute; bottom:-80px; left:-40px; width:260px; height:260px; border-radius:50%; background:rgba(255,255,255,0.04); }}
                    .logo-box {{ display:inline-flex; align-items:center; gap:10px; margin-bottom:28px; position:relative; z-index:1; }}
                    .logo-icon {{ width:44px; height:44px; background:rgba(255,255,255,0.15); border-radius:12px; display:flex; align-items:center; justify-content:center; font-size:22px; }}
                    .logo-text {{ color:#fff; font-size:22px; font-weight:700; letter-spacing:-0.5px; }}
                    .header h1 {{ color:#fff; font-size:28px; font-weight:700; line-height:1.3; position:relative; z-index:1; }}
                    .header p {{ color:rgba(255,255,255,0.75); font-size:15px; margin-top:10px; position:relative; z-index:1; }}
                    .avatar-wrap {{ position:relative; z-index:1; margin:24px auto 0; display:inline-flex; align-items:center; justify-content:center; width:72px; height:72px; border-radius:50%; background:rgba(255,255,255,0.12); border:2px solid rgba(255,255,255,0.25); font-size:36px; }}
                    .wave {{ display:block; background:#1A1028; }}
                    .body {{ padding:40px; }}
                    .greeting {{ font-size:18px; color:#E8D5FF; font-weight:600; margin-bottom:16px; }}
                    .text {{ font-size:15px; color:#A89BC2; line-height:1.7; margin-bottom:20px; }}
                    .access-card {{ background:rgba(113,39,167,0.12); border:1px solid rgba(113,39,167,0.35); border-radius:16px; padding:28px 24px; margin:28px 0; }}
                    .access-card .ac-title {{ font-size:11px; letter-spacing:0.2em; text-transform:uppercase; color:#7A6B95; margin-bottom:18px; }}
                    .access-row {{ display:flex; align-items:center; gap:12px; margin-bottom:14px; }}
                    .access-row:last-child {{ margin-bottom:0; }}
                    .access-icon {{ width:36px; height:36px; flex-shrink:0; background:rgba(113,39,167,0.25); border:1px solid rgba(113,39,167,0.4); border-radius:9px; display:flex; align-items:center; justify-content:center; font-size:16px; }}
                    .access-info {{ flex:1; }}
                    .access-info .label {{ font-size:11px; color:#7A6B95; margin-bottom:2px; }}
                    .access-info .value {{ font-size:15px; font-weight:600; color:#E8D5FF; word-break:break-all; }}
                    .access-info .value.senha {{ letter-spacing:0.15em; }}
                    .perfil-badge {{ display:inline-flex; align-items:center; gap:8px; background:rgba(113,39,167,0.2); border:1px solid rgba(113,39,167,0.4); border-radius:20px; padding:6px 16px; margin:4px 0; }}
                    .perfil-badge span {{ font-size:13px; font-weight:600; color:#C492F0; }}
                    .loja-box {{ background:rgba(255,255,255,0.03); border:1px solid rgba(255,255,255,0.06); border-radius:12px; padding:16px 20px; margin:24px 0; display:flex; align-items:center; gap:14px; }}
                    .loja-icon {{ width:40px; height:40px; flex-shrink:0; background:rgba(113,39,167,0.2); border:1px solid rgba(113,39,167,0.4); border-radius:10px; display:flex; align-items:center; justify-content:center; font-size:18px; }}
                    .loja-text strong {{ display:block; color:#E8D5FF; font-size:14px; font-weight:600; margin-bottom:2px; }}
                    .loja-text span {{ color:#7A6B95; font-size:13px; }}
                    .steps {{ margin:28px 0; }}
                    .step {{ display:flex; align-items:flex-start; gap:14px; margin-bottom:16px; }}
                    .step-num {{ flex-shrink:0; width:28px; height:28px; border-radius:50%; background:rgba(113,39,167,0.25); border:1px solid rgba(113,39,167,0.5); display:flex; align-items:center; justify-content:center; font-size:12px; font-weight:700; color:#C492F0; }}
                    .step-text strong {{ display:block; color:#E8D5FF; font-size:14px; font-weight:600; }}
                    .step-text span {{ color:#7A6B95; font-size:13px; line-height:1.5; }}
                    .alert {{ background:rgba(235,180,0,0.07); border:1px solid rgba(235,180,0,0.25); border-radius:12px; padding:14px 18px; margin:24px 0; display:flex; align-items:flex-start; gap:12px; }}
                    .alert-icon {{ font-size:18px; flex-shrink:0; margin-top:1px; }}
                    .alert-text {{ font-size:13px; color:#C8A84B; line-height:1.6; }}
                    .alert-text strong {{ display:block; color:#EBC84A; margin-bottom:2px; font-size:13px; }}
                    .cta-wrap {{ text-align:center; margin:32px 0; }}
                    .cta {{ display:inline-block; background:linear-gradient(135deg,#7127A7,#A862DC); color:#fff; text-decoration:none; font-size:15px; font-weight:600; padding:16px 40px; border-radius:12px; letter-spacing:0.3px; }}
                    .divider {{ border:none; border-top:1px solid rgba(113,39,167,0.2); margin:28px 0; }}
                    .footer {{ padding:24px 40px 36px; text-align:center; }}
                    .footer p {{ color:#4A3D63; font-size:12px; line-height:1.6; }}
                    .footer a {{ color:#7127A7; text-decoration:none; }}
                    .badge {{ display:inline-block; background:rgba(113,39,167,0.15); border:1px solid rgba(113,39,167,0.3); border-radius:20px; padding:4px 14px; font-size:11px; color:#A862DC; margin-bottom:14px; }}
                </style>
                </head>
                <body>
                    <div class=""wrapper"">
                    <div class=""card"">

                        <div class=""header"">
                            <h1 style=""margin-top:20px;"">Bem-vindo à equipe!</h1>
                            <p>Seu acesso está pronto. Vamos juntos!</p>
                        </div>

                        <svg class=""wave"" viewBox=""0 0 620 40"" xmlns=""http://www.w3.org/2000/svg"" preserveAspectRatio=""none"" height=""40"">
                            <path d=""M0,40 L0,20 Q155,0 310,20 Q465,40 620,20 L620,40 Z"" fill=""#7127A7"" opacity=""0.3""/>
                            <path d=""M0,40 L0,28 Q155,8 310,28 Q465,48 620,28 L620,40 Z"" fill=""#1A1028""/>
                        </svg>

                        <div class=""body"">
                            <p class=""greeting"">Olá, {name}! 👋</p>
                            <p class=""text"">
                                É com muito prazer que te damos as boas-vindas à <strong style=""color:#C492F0"">{nameEmpresa}</strong>. Seu cadastro foi realizado com sucesso no sistema Telemovvi e seu acesso já está liberado. Abaixo estão suas credenciais para o primeiro login.
                            </p>

                            <div class=""access-card"">
                                <p class=""ac-title"">Suas credenciais de acesso</p>

                                <div class=""access-row"">
                                    <div class=""access-icon"">🌐</div>
                                    <div class=""access-info"">
                                        <p class=""label"">Endereço do sistema</p>
                                        <p class=""value""><a href=""{UiURI}"" style=""color:#C492F0;text-decoration:none;"">Acessar</a></p>
                                    </div>
                                </div>

                                <div class=""access-row"">
                                    <div class=""access-icon"">✉️</div>
                                    <div class=""access-info"">
                                        <p class=""label"">Seu e-mail (login)</p>
                                        <p class=""value"">{email}</p>
                                    </div>
                                </div>

                                <div class=""access-row"">
                                    <div class=""access-icon"">🔑</div>
                                    <div class=""access-info"">
                                        <p class=""label"">Senha provisória</p>
                                        <p class=""value senha"">{senhaProvisoira}</p>
                                    </div>
                                </div>
                            </div>

                            <div class=""alert"">
                                <span class=""alert-icon"">⚠️</span>
                                <div class=""alert-text"">
                                    <strong>Altere sua senha no primeiro acesso</strong>
                                    Por segurança, recomendamos que você troque a senha provisória assim que fizer o primeiro login. Nunca compartilhe suas credenciais com ninguém.
                                </div>
                            </div>

                            <div class=""loja-box"">
                                <div class=""loja-icon"">🏪</div>
                                <div class=""loja-text"">
                                    <span>Cargo: {cargo} &nbsp;·</span>
                                </div>
                            </div>

                            <hr class=""divider""/>

                            <p style=""font-size:13px; font-weight:600; color:#E8D5FF; margin-bottom:20px; letter-spacing:0.05em; text-transform:uppercase;"">Primeiros passos</p>
                            <div class=""steps"">
                                <div class=""step"">
                                    <div class=""step-num"">1</div>
                                    <div class=""step-text"">
                                        <strong>Faça seu primeiro login</strong>
                                        <span>Acesse o sistema com o e-mail e a senha provisória informados acima.</span>
                                    </div>
                                </div>
                                <div class=""step"">
                                    <div class=""step-num"">2</div>
                                    <div class=""step-text"">
                                        <strong>Troque sua senha</strong>
                                        <span>Vá em Configurações → Minha conta e defina uma senha pessoal e segura.</span>
                                    </div>
                                </div>
                                <div class=""step"">
                                    <div class=""step-num"">3</div>
                                    <div class=""step-text"">
                                        <strong>Conheça o sistema</strong>
                                        <span>Explore os módulos disponíveis para o seu perfil: vendas, OS, estoque e financeiro.</span>
                                    </div>
                                </div>
                                <div class=""step"">
                                    <div class=""step-num"">4</div>
                                    <div class=""step-text"">
                                        <strong>Fale com o gestor em caso de dúvidas</strong>
                                        <span>Seu responsável pode ajustar permissões e te orientar no uso do dia a dia.</span>
                                    </div>
                                </div>
                            </div>

                            <div class=""cta-wrap"">
                                <a href=""{UiURI}"" class=""cta"">Acessar o sistema agora →</a>
                            </div>

                            <hr class=""divider""/>

                            <p class=""text"" style=""font-size:13px; text-align:center;"">
                                Qualquer dúvida, entre em contato com o suporte ou com o gestor da sua loja. Seja bem-vindo(a) ao time! 💜
                            </p>
                        </div>

                        <div class=""footer"">
                            <span class=""badge"">Telemovvi ERP</span>
                            <p>Este e-mail foi enviado automaticamente após o cadastro de um novo profissional.<br/>
                        </div>
                    </div>
                    </div>
                </body>
                </html>
            ";
        }
    }
}