# Source code of mobile app VerdeNFC

This app allows to clone/create the Mifare(R) Ultralight EV1 NFC tag that are necessary for Bonaverde Berlin raosting/grindind/brewing machine to work after manufacturer's insolvency.
Special thanks to the dedicated Facebook group and especially Hackaverde, Craig Whyte and Michael Grundler who deliverred importing information without this app wouldn't be possible. 

# Building 

This app can be compiled by Visual Studio 2019 with installed Xamarin for Android/iOS feature set. 

# Contributing

Feel free to fork, change and send PR to me. The android and iPhone versions are available through Google Play Store or Apple AppStore.

# Usage

You can either read an existing tag and write it to any tag having still OTP != FFFFFFFF or read it from a file. The file data format is very simple and can be produced by the Android app "Mifare++ Ultra".
By using "speaking" names thus you have a simple browser to a tag library.
Example:

filterreset
```
046EEB09
4ACF5980
5C480000
FFFFFFFF
AA96644B
05AA9641
3278AAAC
5F5005AA
B8414696
376E5A2D
230F0F00
0601FAF8
00000000
00000000
00000000
18000000
000000FF
00050000
00000000
00000000
```
