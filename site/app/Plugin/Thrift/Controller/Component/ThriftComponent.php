<?php

class ThriftComponent extends Component {
    
    var $services;
    
    var $host;
    
    var $transportClass;
    
    var $protocolClass;

    public function __construct(ComponentCollection $collection, $settings = array()) {
        parent::__construct($collection, $settings);

        $GLOBALS['AUTOLOAD_HOOKS'] = isset($settings['hooks']) ? $settings['hooks'] : array();
        $GLOBALS['THRIFT_ROOT'] = isset($settings['root']) ? $settings['root'] : APP . 'Vendor/thrift';
        
        $this->transportClass = isset($settings['transportClass']) ? $settings['transportClass'] : 'TSocket';
        
        $this->protocolClass = isset($settings['protocolClass']) ? $settings['protocolClass'] : 'TBinaryProtocol';
        
        $this->host = $settings['host'];
        
        require_once $GLOBALS['THRIFT_ROOT'] . '/Thrift.php';
        require_once $GLOBALS['THRIFT_ROOT'] . '/autoload.php';
        require_once $GLOBALS['THRIFT_ROOT'] . '/transport/TTransport.php';
        require_once $GLOBALS['THRIFT_ROOT'] . '/transport/TSocket.php';
        require_once $GLOBALS['THRIFT_ROOT'] . '/protocol/TBinaryProtocol.php';
        require_once $GLOBALS['THRIFT_ROOT'] . '/transport/TFramedTransport.php';
        require_once $GLOBALS['THRIFT_ROOT'] . '/transport/TBufferedTransport.php';
        
        $this->services = $settings['services'];
        
        foreach ($settings['services'] as $service => $options) {
            require_once $GLOBALS['THRIFT_ROOT'] . '/packages/service/' . $service . '.php';
        }
    }
    
    /**
     *
     * @param type $service
     * @return TTransport 
     */
    public function getTransportFor($service)
    {
        return new $this->transportClass($this->host, $this->services[$service]['port']);
    }
    
    /**
     *
     * @param type $transport
     * @return TProtocol 
     */
    public function getProtocol($transport)
    {
        return new $this->protocolClass($transport);
    }
}