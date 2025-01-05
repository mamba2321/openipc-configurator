

Vdec Service
/etc/init.d/S98vdec stop
/etc/init.d/S98vdec start

/etc/init.d/S98vdec stop; sleep 1;/etc/init.d/S98vdec start

/etc/vdec.conf

```
osd_elements="-osd_ele1x 910 -osd_ele1y 350 -osd_ele2x 240 -osd_ele2y 350 -osd_ele3x 910 -osd_ele3y 380 -osd_ele4x 40 -osd_ele4y 680 -osd_ele5x 40 -osd_ele5y 650 -osd_ele6x 40 -osd_ele6y 620 -osd_ele7x 40 -osd_ele7y 590 -osd_ele8x 1150 -osd_ele8y 680 -osd_ele9x 1150 -osd_ele9y 590 -osd_ele10x 1050 -osd_ele10y 620 -osd_ele11x 1050 -osd_ele11y 650 -osd_ele12x 350 -osd_ele12y 680 -osd_ele13x 600 -osd_ele13y 680 -osd_ele14x 1100 -osd_ele14y 20 -osd_ele15x 550 -osd_ele15y 30 -osd_ele16x 200 -osd_ele16y 30 -osd_ele17x 1050 -osd_ele17y 560 -osd_ele18x 2 -osd_ele18y 2"
```
              -osd_ele1x 910 -osd_ele1y 350 -osd_ele2x 241 -osd_ele2y 350 -osd_ele3x 910 -osd_ele3y 380 -osd_ele4x 40 -osd_ele4y 680 -osd_ele5x 40 -osd_ele5y 650 -osd_ele6x 40 -osd_ele6y 620 -osd_ele7x 40 -osd_ele7y 590 -osd_ele8x 1150 -osd_ele8y 680 -osd_ele9x 1150 -osd_ele9y 590 -osd_ele10x 1050 -osd_ele10y 620 -osd_ele11x 1050 -osd_ele11y 650 -osd_ele12x 350 -osd_ele12y 680 -osd_ele13x 600 -osd_ele13y 680 -osd_ele14x 1100 -osd_ele14y 20 -osd_ele15x 550 -osd_ele15y 31 -osd_ele16x 200 -osd_ele16y 31 -osd_ele17x 1050 -osd_ele17y 560 -osd_ele18x 2 -osd_ele18y 2


Vdec source: https://github.com/OpenIPC/silicon_research/tree/master/vdec

Grid is 1280x720 regardless of monitor resolution

0,0 off screen
1,1 top left corner

osd_elements="-osd_ele1x 1 -osd_ele1y 1 -osd_ele2x $((1280 - 91)) -osd_ele2y 700"
top left corner
bottom right placement
* grid_width: Total width of the OSD rendering space (e.g., 1280).
* grid_height: Total height of the OSD rendering space (e.g., 720).
* element_width: Width of the element/text (e.g., 91 for SPD:0KM/H).
* bottom_margin: Vertical offset from the bottom edge (e.g., 20 for slight padding).


```ascii
         y ^
           |
           |
x <------------------>
           |
           |
           v
```

osd_elements="
-osd_ele1x 2 -osd_ele1y 2 					# ALT
-osd_ele2x 312 -osd_ele2y 382               # SPD:0KM/H
-osd_ele3x 910 -osd_ele3y 380               # VSPD
-osd_ele4x 40 -osd_ele4y 680                # BAT
-osd_ele5x 40 -osd_ele5y 650                # CONS
-osd_ele6x 35 -osd_ele6y 416                # CURR
-osd_ele7x 35 -osd_ele7y 486                # THR & TEMP (* Bug)
-osd_ele8x 1150 -osd_ele8y 680              # SATS
-osd_ele9x 1150 -osd_ele9y 590              # HDG
-osd_ele10x 1050 -osd_ele10y 620            # LAT
-osd_ele11x 1050 -osd_ele11y 650            # LON
-osd_ele12x 350 -osd_ele12y 680             # DIST
-osd_ele13x 624 -osd_ele13y 659             # RSSI
-osd_ele14x 1100 -osd_ele14y 20             # OpenIPC Logo
-osd_ele15x 550 -osd_ele15y 30              # RX Packets
-osd_ele16x 200 -osd_ele16y 30              # Bit Rate
-osd_ele17x 1050 -osd_ele17y 560            # TIME
-osd_ele18x 486 -osd_ele18y 382"            # HOR


* https://github.com/OpenIPC/silicon_research/issues/54
  [code](https://github.com/OpenIPC/silicon_research/blob/b8753966b76d70ef197f8def2da85ed2aa3c27c3/vdec/main.c#L1241C5-L1244C48)
  
  ```c
  if (osd_element7x > 0){fbg_write(fbg, msg, osd_element7x, osd_element7y*resY_multiplier);}
    sprintf(msg, "TEMP:%.00fC", telemetry_raw_imu/100);
  if (osd_element7x > 0){fbg_write(fbg, msg, osd_element7x, (osd_element7y+30)*resY_multiplier);}
    sprintf(msg, "SATS:%.00f", telemetry_sats);
  ```