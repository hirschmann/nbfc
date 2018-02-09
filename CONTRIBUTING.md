# Contributing

Thank you very much for helping to improve NBFC. :yellow_heart:  
The following is a set of guidelines for contributing to this project.

## Pull requests

Pull requests are always welcome.
By following these guidelines, you can increase the chances that your pull request will be accepted.

### Improvements or bug fixes

- Describe what changes you've made
- Describe why the changes are useful
- Include only relevant changes in your pull request

### Configs

- If your config is based on an existing config, please mention on which one. This can reduce the time which is required to review your config
- Describe how your config works (especially if it contains [RegisterWriteConfigurations](https://github.com/hirschmann/nbfc/wiki/Register-write-configuration) or [FanSpeedPercentageOverrides](https://github.com/hirschmann/nbfc/wiki/Fan-speed-percentage-override))
- Make sure the config doesn't require any modifications to the system (e.g. undervolting, installing 3rd party software, physically modifying the hardware) to work properly

## Issues

If you found a bug or have a suggestion of how to improve NBFC, feel free to create an issue in the issue tracker. It's easier to help you, if you read the [FAQ](https://github.com/hirschmann/nbfc/wiki/FAQ) and respect the following guidelines:

### Bug reports

- Provide information about your system (e.g. notebook model, OS version, installed [software which may interfere with NBFC](https://github.com/hirschmann/nbfc/wiki/FAQ#are-there-any-known-incompatibilities-with-nbfc))
- Provide [logs](https://github.com/hirschmann/nbfc/wiki/Files-and-directories-overview). Please avoid posting the content of the log files. Instead add the files as attachment to the issue
- Try to describe the problem and how to reproduce it as accurately as possible

### Config requests

- Before creating a new issue, give the configs recommended by NBFC a try: `nbfc.exe config --recommend` (for further information, have a look at the [NBFC command line interface](https://github.com/hirschmann/nbfc/wiki/Command-line-interface))
- Don't ask if someone can create a configs for you. Instead, read the tutorial of [how to create a NBFC config](https://github.com/hirschmann/nbfc/wiki/How-to-create-a-NBFC-config) and try to create a config on your own
- In case you've read the tutorial, but you're stuck, please try to provide as much information as possible about your notebook, e.g. manufacturer, model, EC/SuperI/O-chip name, interesting EC registers etc.
- In general, a forum dedicated to your notebook model is probably the best place to find other users who want to help create a config
