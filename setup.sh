mkdir tmp &&
cd tmp &&
curl -sSL https://github.com/glfw/glfw/releases/download/3.3.4/glfw-3.3.4.bin.WIN64.zip > winglfw.zip &&
unzip winglfw.zip &&
rm winglfw.zip &&
mkdir -p ../libs/win &&
cp ./glfw-3.3.4.bin.WIN64/lib-vc2019/glfw3.dll ../libs/win/glfw3.dll &&

cd .. &&
rm -rf ./tmp

