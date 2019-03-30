## 1. Install mono
```
sudo apt install gnupg ca-certificates
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt update
```


## 2. Clone repository anywhere
```
https://github.com/hirschmann/nbfc.git
```

## 3. Manually download `nuget.exe` and paste to nbfc (latest stable):

[NUGET](https://www.nuget.org/downloads)

and run:
```
sudo mono nuget.exe restore
```

## 4. Delete `build.sh` from root folder and paste edited `Ubuntu18/build.sh`, then run:

```
sudo sh build.sh
```

## 5. Run `nbfcservice.sh`

```
sudo sh nbfcservice.sh
```

## 6. Copy the files from `nbfc/Linux/bin/Release/` to `/opt/nbfc/`

```
sudo cp -rf nbfc/Linux/bin/Release/ /opt/nbfc/
```

## 7. Copy `nbfc/Linux/nbfc.service` and `nbfc/Linux/nbfc-sleep.service` into `/etc/systemd/system/`

```
sudo cp nbfc/Linux/nbfc.service /opt/nbfc/
sudo cp nbfc/Linux/nbfc-sleep.service /opt/nbfc/
```

## 8. To enable and start the service:

```
sudo systemctl enable nbfc --now
```

## 9. CHECK FOLDER Config in `/opt/nbfc/Config` and choose file for your laptop

```
cd Config
ls
```

EG:

```
mono nbfc.exe config --apply Asus\ ROG\ G751JL
```

## 10. Go back to `/opt/nbfc/`

```
mono nbfc.exe config --apply 'Your config file'
```

## 11. Check it:

```
mono nbfc.exe status -a
```

## 12. **Buy me beer ;)**