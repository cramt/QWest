# frozen_string_literal: true

require 'pstore'

module Utils
    def search(name)
        store = PStore.new 'cache.pstore'
        name = URI.encode(name)
        offset = 0
        res = []
        until offset.nil?
            url = "https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&srsearch=#{name}&sroffset=#{offset}"
            result = nil
            store.transaction(true) do
                result = store[url]
            end
            unless result
                result = fetch url
                store.transaction do
                    store[url] = result
                end
            end
            offset = result[:continue]
            offset = offset[:sroffset] if offset
            if result[:query]
                res.concat result[:query][:search]
            end
        end
        res
    end

    def fetch(url)
        result = nil
        while result.nil?
            body = HTTParty.get(url).body.to_s
            begin
                result = JSON.parse(body, symbolize_names: true)
            rescue StandardError
            end
        end
        result
    end

    module_function :search
    module_function :fetch
    end
