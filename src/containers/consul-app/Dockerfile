# escape=

FROM isago.ch/gbo/ocelot-dc-lab/app

# Add Consul from https://github.com/hashicorp/docker-consul/blob/release/1.8.x/0.X/Dockerfile
# FROM alpine:3.12
LABEL org.opencontainers.image.authors="isago.ch consul container decorating asp.net app"

# This is the release of Consul to pull in.
ARG CONSUL_VERSION=1.8.6
LABEL org.opencontainers.image.version=$CONSUL_VERSION

# This is the location of the releases.
ENV HASHICORP_RELEASES=https://releases.hashicorp.com

# Create a consul user and group first so the IDs get set the same way, even as
# the rest of this may change over time.
RUN addgroup consul && \
    # adduser --system --ingroup consul consul 
    adduser -S -G consul consul

# Set up certificates, base tools, and Consul.
# libc6-compat is needed to symlink the shared libraries for ARM builds
RUN set -eux && \
    apk add --no-cache ca-certificates curl dumb-init gnupg libcap openssl su-exec iputils jq libc6-compat iptables && \
    gpg --keyserver pool.sks-keyservers.net --recv-keys C874011F0AB405110D02105534365D9472D7468F && \
    mkdir -p /tmp/build && \
    cd /tmp/build && \
    apkArch="$(apk --print-arch)" && \
    case "${apkArch}" in \
        aarch64) consulArch='arm64' ;; \
        armhf) consulArch='armhfv6' ;; \
        x86) consulArch='386' ;; \
        x86_64) consulArch='amd64' ;; \
        *) echo >&2 "error: unsupported architecture: ${apkArch} (see ${HASHICORP_RELEASES}/consul/${CONSUL_VERSION}/)" && exit 1 ;; \
    esac && \
    wget ${HASHICORP_RELEASES}/consul/${CONSUL_VERSION}/consul_${CONSUL_VERSION}_linux_${consulArch}.zip && \
    wget ${HASHICORP_RELEASES}/consul/${CONSUL_VERSION}/consul_${CONSUL_VERSION}_SHA256SUMS && \
    wget ${HASHICORP_RELEASES}/consul/${CONSUL_VERSION}/consul_${CONSUL_VERSION}_SHA256SUMS.sig && \
    gpg --batch --verify consul_${CONSUL_VERSION}_SHA256SUMS.sig consul_${CONSUL_VERSION}_SHA256SUMS && \
    grep consul_${CONSUL_VERSION}_linux_${consulArch}.zip consul_${CONSUL_VERSION}_SHA256SUMS | sha256sum -c && \
    unzip -d /bin consul_${CONSUL_VERSION}_linux_${consulArch}.zip && \
    cd /tmp && \
    rm -rf /tmp/build && \
    gpgconf --kill all && \
    apk del gnupg openssl && \
    rm -rf /root/.gnupg && \
# tiny smoke test to ensure the binary we downloaded runs
    consul version

# The /consul/data dir is used by Consul to store state. The agent will be started
# with /consul/config as the configuration directory so you can add additional
# config files in that location.
RUN mkdir -p /consul/data && \
    mkdir -p /consul/config && \
    chown -R consul:consul /consul

# set up nsswitch.conf for Go's "netgo" implementation which is used by Consul,
# otherwise DNS supercedes the container's hosts file, which we don't want.
RUN test -e /etc/nsswitch.conf || echo 'hosts: files dns' > /etc/nsswitch.conf

# Expose the consul data directory as a volume since there's mutable state in there.
VOLUME /consul/data

# Server RPC is used for communication between Consul clients and servers for internal
# request forwarding.
EXPOSE 8300

# Serf LAN and WAN (WAN is used only by Consul servers) are used for gossip between
# Consul agents. LAN is within the datacenter and WAN is between just the Consul
# servers in all datacenters.
EXPOSE 8301 8301/udp 8302 8302/udp

# HTTP and DNS (both TCP and UDP) are the primary interfaces that applications
# use to interact with Consul.
EXPOSE 8500 8600 8600/udp