Wspbuilder.exe

stsadm -o retractsolution -name SharePointPrimitives.SettingsProvider.Stsadm.wsp -local
stsadm -o execadmsvcjobs
stsadm -o deletesolution -name  SharePointPrimitives.SettingsProvider.Stsadm.wsp

stsadm -o addsolution -filename SharePointPrimitives.SettingsProvider.Stsadm.wsp
stsadm -o deploysolution -name sharepointprimitives.settingsprovider.stsadm.wsp -local -allowgacdeployment
stsadm -o execadmsvcjobs