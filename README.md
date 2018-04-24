ChannelSplit 1.0.0.0

Useage (-options) arg1-argx

Args may be individual image, files, argument files,directories or web url, zipfiles if chosen or individual files will be placed in the same folder as original image

Images that were downloaded from url and clipboard images will be saved to thisappdir\ChannelSplit\ChannelSplit\bin\Debug\imgdownloads

--- OPTIONS ---

-a Copies Alpha Channel

-A Creates a color overlay in the color chosen

-c Copies cyan Channel

-m Copies magenta Channel

-y Copies yellow Channel

-k Copies black Channel

-r Copies redblack Channel

-R Copies redwhite Channel

-g Copies greenblack Channel

-G Copies greenwhite Channel

-b Copies blueblack Channel

-B Copies bluewhite Channel

-u Copies hue Channel

-t Copies brightness Channel

-T Copies saturation Channel

-e Copies grayscale Channel

-f copy files rather than zip them up

-i invert image color

-o on downloaded images, copys only downloaded original

-C use image stored in clipboard

Misc ----



EX: ChannelSplit -AcR image.jpg "C:\imgdir" ....

Argument files are just a list of files in a text file, so it would be file,url,or directory then \n, no options amy be specifed in argument file

Ex: how to make argument file

Ex: open new text then fill it with one arg per line, no options

Ex: image.jpg\n

Ex: dir\n

Ex: imageurl\n

save text file as .arg and then load in program
