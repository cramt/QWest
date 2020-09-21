# frozen_string_literal: true

module Utils
    def search(name)
        name = URI.encode(name)
        offset = 0
        res = []
        until offset.nil?
            result = fetch "https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&srsearch=#{name}&sroffset=#{offset}"
            offset = result[:continue]
            offset = offset[:sroffset] if offset
            res.concat result[:query][:search]
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
