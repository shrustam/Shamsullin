%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil /i Shamsullin.WcfCache.exe
sc config Shamsullin.WcfCache start= auto
net start "Shamsullin.WcfCache"