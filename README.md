ChannelSplit 1.0.0.0

Useage (-options) (hexcode) (negative) arg1-argx

Args may be individual image, files, argument files,directories or web url, zipfiles if chosen or individual files will be placed in the same folder as original image

Images that were downloaded from url and clipboard images will be saved to thisapp\imgdownloads

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

-L Colorizes an image with a color hexcode, only images after hexcode will be colorized, works best with darker colors light ones produce white pictures, place hexcode in arguments optional keyword "negative" may be used, int that case subtracts that color from image

-F Shifts image color with a color filter , place hexcode in arguments, darker hexcodes work better light hexcodes produce white pictures, optional keyword "negative" may be used, in that case subtracts that color from image

-i invert image color

-o on downloaded images, copys only downloaded original

-C use image stored in clipboard

-S takes a sample of image with threshold 0-255, add to the program arguments, first will set min thresh and second will set to max thresh

-G takes a sample of image with threshold 0-255, same as threshold except does it in color, determines by pixel brightness

-n uses edge detection and makes a bad fax or pencil drawing

-M Uses a color matrix, much faster to split the rgbblack channels and grayscale, no alpha channels versions available this way

-x adjusts exposeure 0-100% or -0-100% ,0 is middle and no adjustment , add percentage to arguments

Misc ----



EX:threshold example -S 100 250 image.jpg

EX: ChannelSplit -AcR image.jpg "C:\imgdir" ....

Argument files are just a list of files in a text file, so it would be file,url,or directory then \n, no options any be specifed in argument file

Ex: how to make argument file

Ex: open new text then fill it with one arg per line, no options

Ex: image.jpg\n

Ex: dir\n

Ex: imageurl\n

save text file as .arg and then load in program
