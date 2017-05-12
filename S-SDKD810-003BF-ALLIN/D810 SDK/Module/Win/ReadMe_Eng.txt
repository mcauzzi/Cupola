Nikon Type0014 Module SDK Revision.5.0 summary


Usage
 Control the camera.


Supported camera
 D810, D810A


Environment of operation
 [Windows]
    Windows 7 (SP1) 32bit/64bit edition
    (Home Basic / Home Premium / Professional / Enterprise / Ultimate)
    Windows 8.1 32bit/64bit edition
    (Windows 8.1 / Pro / Enterprise) 
    Windows 10 32bit/64bit edition

 [Macintosh]
    Mac OS X 10.9.5 (Mavericks)
    Mac OS X 10.10.5 (Yosemite)
    Mac OS X 10.11.2 (El Capitan)
    *  64bit mode only (32bit mode is not supported)

Contents
 [Windows]
    Documents
      MAID3(E).pdf : Basic interface specification
      MAID3Type0014(E).pdf : Extended interface specification used 
                                                              by Type0014 Module
      Usage of Type0014 Module(E).pdf : Notes for using Type0014 Module
      Type0014 Sample Guide(E).pdf : The usage of a sample program

    Binary Files
      Type0014.md3 : Type0014 Module for Win
      NkdPTP.dll : Driver for PTP mode used by Win

    Header Files
      Maid3.h : Basic header file of MAID interface
      Maid3d1.h : Extended header file for Type0014 Module
      NkTypes.h : Definitions of the types used in this program.
      NkEndian.h : Definitions of the types used in this program.
      Nkstdint.h : Definitions of the types used in this program.

    Sample Program
      Type0014CtrlSample(Win) : Project for Microsoft Visual Studio 2013


 [Macintosh]
    Documents
      MAID3(E).pdf : Basic interface specification
      MAID3Type0014(E).pdf : Extended interface specification used by 
                                                                Type0014 Module
      Usage of Type0014 Module(E).pdf : Notes for using Type0014 Module
      Type0014 Sample Guide(E).pdf : The usage of a sample program
      [Mac OS] Notice about using Module SDK(E).txt : Notes for using SDK
                                                                on Mac OS

    Binary Files
      Type0014 Module.bundle : Type0014 Module for Mac
      libNkPTPDriver2.dylib : PTP driver for Mac 
 
    Header Files
      Maid3.h : Basic header file of MAID interface
      Maid3d1.h : Extended header file for Type0014 Module
      NkTypes.h : Definitions of the types used in this program.
      NkEndian.h : Definitions of the types used in this program.
      Nkstdint.h : Definitions of the types used in this program.

    Sample Program
      Type0014CtrlSample(Mac) : Sample program project for Xcode 6.2.


Limitations
 This module cannot control two or more cameras.
