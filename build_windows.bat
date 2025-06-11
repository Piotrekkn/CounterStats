set mingw_dir=C:\msys64\mingw64
set build_dir=windows_build
@echo off
::clear build directory
RD /S /Q %build_dir%
::build
dotnet publish -c Release -o %build_dir% -r win-x64 -p:PublishReadyToRun=true -p:PublishSingleFile=true --self-contained true

::copy files needed to run the program on windows with out minigw 
::data
xcopy data_win %build_dir% /s /e

::icons
xcopy %mingw_dir%\share\icons\Adwaita %build_dir%\share\icons\Adwaita\  /s /e
xcopy %mingw_dir%\share\icons\AdwaitaLegacy %build_dir%\share\icons\AdwaitaLegacy\  /s /e
xcopy %mingw_dir%\share\icons\hicolor %build_dir%\share\icons\hicolor\  /s /e

::libs
xcopy %mingw_dir%\lib\gdk-pixbuf-2.0 windows_build\lib\gdk-pixbuf-2.0\  /s /e
::list of libs needed to run
set list=libsystre-0.dll libgirepository-2.0-0.dll libgirepository-1.0-1.dll libyaml-0-2.dll libasprintf-0.dll libthai-0.dll libffi-8.dll libharfbuzz-gobject-0.dll libssl-3-x64.dll libbz2-1.dll libpcre2-8-0.dll libepoxy-0.dll libgmpxx-4.dll libgdk_pixbuf-2.0-0.dll libcairo-script-interpreter-2.dll libpcre2-16-0.dll libiconv-2.dll libtasn1-6.dll libwinpthread-1.dll libnghttp2-14.dll libngtcp2_crypto_ossl.dll libpcre2-32-0.dll libgmp-10.dll libsqlite3-0.dll libtre-5.dll libwebpdemux-2.dll libpango-1.0-0.dll libwebpdecoder-3.dll libmpdec-4.dll libpcre2-posix-3.dll libhistory8.dll libjpeg-8.dll libnghttp3-9.dll libgif-7.dll libmpdec++-4.dll libgnutls-30.dll libcharset-1.dll libdeflate.dll libp11-kit-0.dll libgthread-2.0-0.dll libtiff-6.dll libtermcap-0.dll libbrotlidec.dll libgraphene-1.0-0.dll libwebp-7.dll libglib-2.0-0.dll libnettle-8.dll libpng16-16.dll libstdc++-6.dll libpsl-5.dll libjson-glib-1.0-0.dll libadwaita-1-0.dll libbrotlienc.dll libpangoft2-1.0-0.dll libcares-2.dll libtiffxx-6.dll libngtcp2-16.dll libncurses++w6.dll libsharpyuv-0.dll libgmodule-2.0-0.dll libcairo-gobject-2.dll libgraphite2.dll libappstream-5.dll zlib1.dll libssh2-1.dll libwebpmux-3.dll librsvg-2-2.dll libquadmath-0.dll libncursesw6.dll libharfbuzz-subset-0.dll libjbig-0.dll libexpat-1.dll libfreetype-6.dll libxml2-2.dll libbrotlicommon.dll liblzo2-2.dll libpixman-1-0.dll libgcc_s_seh-1.dll libcrypto-3-x64.dll libzstd.dll libharfbuzz-0.dll libfribidi-0.dll libLerc.dll libreadline8.dll libgobject-2.0-0.dll libdatrie-1.dll libpanelw6.dll liblzma-5.dll libturbojpeg.dll libmenuw6.dll libidn2-0.dll libhogweed-6.dll libfontconfig-1.dll libpangowin32-1.0-0.dll libgtk-4-1.dll libcairo-2.dll libgnutlsxx-30.dll libformw6.dll libgio-2.0-0.dll libcurl-4.dll libunistring-5.dll libatomic-1.dll libpangocairo-1.0-0.dll libngtcp2_crypto_gnutls-8.dll libgnutls-openssl-27.dll libintl-8.dll libxmlb-2.dll libgomp-1.dll
end of list
(for %%a in (%list%) do ( 
   xcopy "%mingw_dir%\bin\%%a" %build_dir%
))
::cls
echo Your build with dlls needed to run is in %build_dir%

echo Run CounterStats with: ./%build_dir%/CounterStats.exe