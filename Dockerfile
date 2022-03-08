FROM mono:6.12

RUN apt update && apt install --no-install-recommends -y wget

WORKDIR /nbfc

CMD ./build.sh
