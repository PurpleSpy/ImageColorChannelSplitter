ChannelSplit 1.0.0.0

ChannelSplit (options) (command modifers) arg1-argx



[program options]

Args may be individual image, files, argument files,directories or web url, zipfiles if chosen or individual files will be placed in the same folder as original image

Images that were downloaded from url and clipboard images will be saved to thisapplocation\imgdownloads

-C Uses Image From clipboad

-M Uses a color matrix to do grayscale and rgb split, faster than other method

-a Copies the alpha channel as a grayscale image

-A Copies the alpha channel in the color of your choice as an image with transparency

-c Grabs the Cyan color channel for black or white backgrounds

-m Grabs the Magenta color channel for black or white backgrounds

-y Grabs the Yellow color channel for black or white backgrounds

-k Grabs the Yellow color channel for black or white backgrounds

-r Grabs the Red color channel for black or white backgrounds

-g Grabs the Green color channel for black or white backgrounds

-b Grabs the Blue color channel for black or white backgrounds

-u Grabs the hue of image

-t Grabs the brightness of image

-T Grabs the saturation of image

-e Makes a grayscale version of the image

-o For web or clipboard images, only downloads original image and nothing more

-f Instead of making a zipfile of all the channel images, copies them to the directory of original image instead

-L Takes a grayscale version the image and then applies a color filter to color the image, takes color aa a hexcode as argument

-F Takes the image and applies a color filter over it, takes color aa a hexcode as argument

-z Selects a random color to be used with colorize and colorshift

-i Inverts the image colors

-S Takes a measurement of image brightness per pixel with a min and max threshold, writes a grayscale image between threshold, can take two arguments from 0-255

-n Draws a outline of image, threshold for brightness and pixel sample size must be set, because of how long it takes this option must be specifed alone

-G Same as threshold except does it in color, looks like a bad photoshop cutout

-x Adjusts the exposure of a picture from 100%-0%-100% with 0% being no change, takes one argument in percent

-h or -? or /h or /? shows this help page





[command modifiers]

command modifiers are how you specify settings for the filters they are just added to arguments



b+/-100-0% sets the exposure brightness "EX: b50%"

rx X is how many pixels is your sample size in outline draw, larger sample size and lower threshold results in smoother image but slow processs time "EX:  r50", at 200 and above limited to one image and images can take 5mins or more to create

t100-0% sets threshold tolerance for line draw, optimal is about 2%

c+/-#hexcode sets the hexcode used for colorize function, may be positive or negative "EX: c#AD321A" OR "EX: c-#cdcdcd"

C+/-#hexcode sets the hexcode used for colorshift function, may be positive or negative "EX: C#AD321A" OR "EX: C-#cdcdcd"



[argument files]

Argument files are just a list of files in a text file, so it would be file,url,or directory then return one per line, no options or command modifiers can be specified argument file

....how to make argument file...

open new text then fill it with one arg per line, no options then save as with the extention arg



[useage example]

EX: ChannelSplit -AcR image.jpg "C:\imgdir" ...."
