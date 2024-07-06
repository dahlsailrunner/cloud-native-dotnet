## Getting Started


Copy `podman.exe` to `docker.exe`.

```powershell
New-LocalGroup -Name "docker-users"
net localgroup docker-users dahls /ADD
```

