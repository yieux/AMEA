mv /home/yangxy/example/0025f94ed7defb1f063b30463612f6795c0c5aa0786d51f49722e17962d2bae2.apk /home/yangxy/example/app20160927mx.apk
if [ ! -d "/home/yangxy/example/00/app20160927mx" ]; then
  mkdir -p /home/yangxy/example/00/app20160927mx
fi
cd /home/yangxy/example/00/app20160927mx
mv ../../app20160927mx.apk ./app20160927mx.apk
if [ ! -d "./IC3" ]; then
  rm -rf ./IC3
fi
mkdir ./IC3
cd ./IC3
/home/yangxy/tool/ic3/dare-1.1.0-linux/dare -d /home/yangxy/example/00/app20160927mx/IC3/dareout /home/yangxy/example/00/app20160927mx/app20160927mx.apk
mkdir ./ic3result
java -jar /home/yangxy/tool/ic3/ic3-0.2.0/ic3-0.2.0-full.jar -apkormanifest ../app20160927mx.apk -input ./dareout/retargeted/app20160927mx -cp /home/yangxy/tool/ic3/ic3-0.2.0/android.jar -out ./ic3result -protobuf ./ic3result