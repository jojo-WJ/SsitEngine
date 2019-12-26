# -*- coding: utf-8 -*-

import os
import shutil
import codecs

inputDir = os.path.join( os.getcwd(), "protos" )
outputDir = os.path.join( os.getcwd(), "out\\cs" )

for dirpath, dirnames, filenames in os.walk( inputDir ):

    for filename in filenames:

        pathfile = os.path.join(dirpath, filename)

        if os.path.isfile( pathfile ):

            handle = codecs.open( pathfile, "r", 'utf-8' )
            firstline = handle.next()
            handle.close()

            if firstline.startswith( "package " ) and ( firstline.endswith( ";\r\n" ) or firstline.endswith( ";" ) ):
                # Todo 通过正则表达式取出包名
                package = firstline.split( " " )[1].split(";")[0].split(";\r\n")[0]
                packagedir = package.replace( ".", "\\" )

                targetDir = os.path.join( outputDir, packagedir )
                if not os.path.exists(targetDir):
                    os.makedirs(targetDir)

                source = os.path.join( outputDir, filename ).replace( ".proto", ".cs" )
                target = os.path.join( targetDir, filename ).replace( ".proto", ".cs" )

                if os.path.isfile( source ):
                    shutil.copy( source, target )
                    print( "copy: " + source + "\nto  : " + target )

            # 输出字符串发现字符结尾是\r\n
            # [u'package SSIT.stageproto', u'\r\n']
            # print( firstline.split( ";" ) )
