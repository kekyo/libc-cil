#!/bin/bash

cd crt0

gzip -9 < crt0_i.s > crt0_i.o
gzip -9 < crt0_icv.s > crt0_icv.o
gzip -9 < crt0_v.s > crt0_v.o
gzip -9 < crt0_vcv.s > crt0_vcv.o
