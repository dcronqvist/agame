mkdir tmp &&
cd tmp &&
curl -sSL https://github.com/glfw/glfw/releases/download/3.3.4/glfw-3.3.4.bin.MACOS.zip > macglfw.zip &&
unzip macglfw.zip &&
rm macglfw.zip &&
mkdir -p ../libs/mac &&
cp ./glfw-3.3.4.bin.MACOS/lib-universal/*.* ../libs/mac/ &&

cd .. &&
rm -rf ./tmp

