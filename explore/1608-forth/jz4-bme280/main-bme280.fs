\ frozen application, this runs tests and wipes to a clean slate if they pass

compiletoram? [if]  forgetram  [then]

0 constant DEBUG  \ 0 = send RF packets, 1 = display on serial port
10 constant RATE  \ seconds between readings


: .00 ( n -- ) 0 swap 0,5 d+ 0,01 f* 2 f.n ;

: show-readings ( vprev vcc tint humi pres temp -- )
  hwid hex. ." = "
  .00 ." °C, " .00 ." hPa, " .00 ." %RH, "
  . ." °C, " . ." => " . ." mV " ;

: send-packet ( vprev vcc tint humi pres temp -- )
  \ 2 <pkt hwid u+> u+> 5 0 do u+> loop pkt>rf ;
2 <pkt hwid +pkt +pkt 5 0 do +pkt loop pkt>rf ;

: low-power-sleep
  rf-sleep
  adc-deinit \ only-msi
  RATE 0 do stop1s loop
  hsi-on adc-init ;

: main
  2.1MHz  1000 systick-hz  lptim-init i2c-init adc-init

  8686 rf.freq ! 42 rf.group ! 62 rf.nodeid !
  rf-init 30 rf-power
  OMODE-PP PA14 io-mode!  PA14 ioc!  \ set PA14 to "0", acting as ground
  OMODE-PP PA13 io-mode!  PA13 ios!  \ set PA13 to "1", acting as +3.3V

  bme-init drop bme-calib
\  tsl-init drop

  begin
    led-off

    adc-vcc                      ( vprev )
    low-power-sleep
    adc-vcc adc-temp             ( vprev vcc tint )
\    tsl-data  bme-data bme-calc  ( vprev vcc tint lux humi pres temp )
    bme-data bme-calc
    led-on

    DEBUG if
      show-readings cr 1 ms
    else
      \ show-readings cr 1 ms
      send-packet
    then
  key? until ;

