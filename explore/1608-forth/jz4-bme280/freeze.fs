\ frozen application, this runs tests and wipes to a clean slate if they pass

include always.fs
include board.fs
include core.fs

compiletoflash
include main-bme280.fs

\ run tests, even when connected (especially so, in fact!)
: init init unattended main ;
