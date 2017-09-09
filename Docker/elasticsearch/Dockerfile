FROM  docker.elastic.co/elasticsearch/elasticsearch:5.5.0

# Copy over the config file
ADD ./config/elasticsearch.yml /usr/share/elasticsearch/config/elasticsearch.yml

# Install the attachment pluging to unpack files content
RUN bin/elasticsearch-plugin install ingest-attachment