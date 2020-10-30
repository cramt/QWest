# frozen_string_literal: true

require 'json'
require 'httparty'
require './fetch'

countries_str = File.read 'iso3166.json'
countries_str_extra_names = File.read 'iso3166-1.json'
countries = JSON.parse(countries_str, symbolize_names: true)
countries_extra_names = JSON.parse(countries_str_extra_names, symbolize_names: true)[:"3166-1"].inject({}) {|acc, x|
    acc[x[:alpha_2].to_sym] = x
    acc
}
countries = countries.map do |country|
    country[:alpha_2] = country[:"alpha-2"]
    country.delete(:"alpha-2")
    country[:alpha_3] = country[:"alpha-3"]
    country.delete(:"alpha-3")
    country[:country_code] = country[:"country-code"]
    country.delete(:"country-code")
    country[:iso_3166_2] = country[:"iso_3166-2"]
    country.delete(:"iso_3166-2")
    country[:sub_region] = country[:"sub-region"]
    country.delete(:"sub-region")
    country[:intermediate_region] = country[:"intermediate-region"]
    country.delete(:"intermediate-region")
    country[:region_code] = country[:"region-code"]
    country.delete(:"region-code")
    country[:sub_region_code] = country[:"sub-region-code"]
    country.delete(:"sub-region-code")
    country[:intermediate_region_code] = country[:"intermediate-region-code"]
    country.delete(:"intermediate-region-code")
    other_country = countries_extra_names[country[:alpha_2].to_sym]
    country[:names] = [country[:name], other_country[:name], other_country[:official_name], other_country[:common_name]].uniq.compact
    country[:name] = country[:names].min_by(&:length)

    country[:subdivisions] = Fetch.fetch(country[:alpha_2])
    puts("finished " + country[:name])
    country
end
File.write 'countries.json', countries.to_json
